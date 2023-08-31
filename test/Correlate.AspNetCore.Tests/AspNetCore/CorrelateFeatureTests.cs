using Correlate.AspNetCore.Middleware;
using Correlate.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;

namespace Correlate.AspNetCore;

public sealed class CorrelateFeatureTests : IDisposable
{
    private readonly DefaultHttpContext _httpContext;
    private readonly CorrelateFeature _sut;
    private readonly TestResponseFeature _responseFeature;
    private readonly IActivity _activityMock;
    private readonly IActivityFactory _activityFactoryMock;
    private readonly CorrelateOptions _options;
    private readonly ICorrelationIdFactory _correlationIdFactory;

    private static readonly string CorrelationId = Guid.NewGuid().ToString("D");

    public CorrelateFeatureTests()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Features.Set<IHttpResponseFeature>(_responseFeature = new TestResponseFeature());

        Logger serilog = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestCorrelator()
            .CreateLogger();
        var loggerFactory = new LoggerFactory(new[] { new SerilogLoggerProvider(serilog) });

        _activityMock = Substitute.For<IActivity>();
        _activityFactoryMock = Substitute.For<IActivityFactory>();
        _activityFactoryMock.CreateActivity().Returns(_activityMock);

        _correlationIdFactory = Substitute.For<ICorrelationIdFactory>();
        _correlationIdFactory.Create().Returns(CorrelationId);

        _options = new CorrelateOptions();
        _sut = new CorrelateFeature(loggerFactory, _correlationIdFactory, _activityFactoryMock, Options.Create(_options));
    }

    public void Dispose()
    {
        _responseFeature.Dispose();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("X-Custom-Header")]
    public async Task Given_that_correlating_has_started_when_firing_to_send_headers_it_should_add_correlationId_header_to_response(string? requestHeader)
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();
        if (requestHeader is not null)
        {
            _options.RequestHeaders = new[] { requestHeader };
        }

        var expectedHeader = new KeyValuePair<string, StringValues>(
            _options.RequestHeaders[0],
            new StringValues(CorrelationId)
        );

        // Act
        _sut.StartCorrelating(_httpContext);
        await _responseFeature.FireOnSendingHeadersAsync();

        // Assert
        _httpContext.Response.Headers.Should().Contain(expectedHeader);
        _correlationIdFactory.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
    }

    [Theory]
    [InlineData(CorrelationHttpHeaders.CorrelationId)]
    [InlineData(CorrelationHttpHeaders.RequestId)]
    public async Task Given_that_request_contains_correlationId_header_in_allowed_list_when_correlating_has_started_it_should_have_used_that_correlationId(string headerName)
    {
        _options.RequestHeaders = new[] { CorrelationHttpHeaders.CorrelationId, CorrelationHttpHeaders.RequestId };

        string correlationId = Guid.NewGuid().ToString("D");
        _httpContext.Features.Get<IHttpRequestFeature>()!
            .Headers[headerName] = correlationId;

        var expectedHeader = new KeyValuePair<string, StringValues>(
            headerName,
            correlationId
        );

        // Act
        _sut.StartCorrelating(_httpContext);
        await _responseFeature.FireOnSendingHeadersAsync();

        // Assert
        _httpContext.Response.Headers.Should().Contain(expectedHeader);
        _correlationIdFactory.DidNotReceive().Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(correlationId);
    }

    [Theory]
    [MemberData(nameof(CtorNullArgumentTestCases))]
    public void Given_that_required_arg_is_null_argument_when_creating_instance_it_should_throw
    (
        ILoggerFactory loggerFactory,
        ICorrelationIdFactory correlationIdFactory,
        IActivityFactory activityFactory,
        IOptions<CorrelateOptions> options,
        string expectedParamName,
        Type expectedExceptionType
    )
    {
        // Act
        Func<CorrelateFeature> act = () => new CorrelateFeature(loggerFactory, correlationIdFactory, activityFactory, options);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithParameterName(expectedParamName)
            .Which.Should()
            .BeOfType(expectedExceptionType);
    }

    public static IEnumerable<object?[]> CtorNullArgumentTestCases()
    {
        ILoggerFactory loggerFactory = Substitute.For<ILoggerFactory>();
        ICorrelationIdFactory correlationIdFactory = Substitute.For<ICorrelationIdFactory>();
        IActivityFactory activityFactory = Substitute.For<IActivityFactory>();
        IOptions<CorrelateOptions> options = Substitute.For<IOptions<CorrelateOptions>>();

        yield return new object?[] { null, correlationIdFactory, activityFactory, options, nameof(loggerFactory), typeof(ArgumentNullException) };
        yield return new object?[] { loggerFactory, null, activityFactory, options, nameof(correlationIdFactory), typeof(ArgumentNullException) };
        yield return new object?[] { loggerFactory, correlationIdFactory, null, options, nameof(activityFactory), typeof(ArgumentNullException) };
        yield return new object?[] { loggerFactory, correlationIdFactory, activityFactory, null, nameof(options), typeof(ArgumentException) };
    }

    [Fact]
    public void Given_that_activity_in_items_was_replaced_with_something_else_when_correlating_has_stopped_it_should_not_throw()
    {
        _sut.StartCorrelating(_httpContext);
        _httpContext.Items.Should().ContainKey(CorrelateFeature.RequestActivityKey);
        _httpContext.Items[CorrelateFeature.RequestActivityKey] = new object();

        // Act
        Action act = () => _sut.StopCorrelating(_httpContext);

        // Assert
        act.Should().NotThrow();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
        _activityMock.DidNotReceive().Stop();
    }

    [Fact]
    public void Given_that_activity_is_not_in_items_when_correlating_has_stopped_it_should_not_throw()
    {
        _sut.StartCorrelating(_httpContext);
        _httpContext.Items.Should().ContainKey(CorrelateFeature.RequestActivityKey);
        _httpContext.Items.Clear();

        // Act
        Action act = () => _sut.StopCorrelating(_httpContext);

        // Assert
        act.Should().NotThrow();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
        _activityMock.DidNotReceive().Stop();
    }

    [Fact]
    public async Task Given_that_correlating_has_not_started_when_firing_to_send_headers_it_should_not_add_correlationId_header_to_response()
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();

        // Act
        await _responseFeature.FireOnSendingHeadersAsync();

        // Assert
        _httpContext.Response.Headers.Should().BeEmpty();
    }

    [Fact]
    public async Task Given_that_response_already_contains_correlation_header_when_firing_to_send_headers_it_should_not_overwrite_the_correlationId_header()
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();

        const string existingCorrelationId = "existing-id";
        _responseFeature.Headers.Add(_options.RequestHeaders[0], existingCorrelationId);

        var expectedHeader = new KeyValuePair<string, StringValues>(
            _options.RequestHeaders[0],
            existingCorrelationId
        );

        // Act
        _sut.StartCorrelating(_httpContext);
        await _responseFeature.FireOnSendingHeadersAsync();

        // Assert
        _httpContext.Response.Headers.Should().Contain(expectedHeader);
        _correlationIdFactory.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(Arg.Any<string>());
    }

    [Fact]
    public async Task When_correlating_has_started_it_should_create_logScope_with_correlationId()
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();
        const string expectedLogProperty = CorrelateConstants.CorrelationIdKey;

        // Act
        using (TestCorrelator.CreateContext())
        {
            _sut.StartCorrelating(_httpContext);
            await _responseFeature.FireOnSendingHeadersAsync();

            // Assert
            TestCorrelator.GetLogEventsFromCurrentContext()
                .Should()
                .ContainSingle(le => le.MessageTemplate.Text.StartsWith("Setting response header"))
                .Which.Properties
                .Should()
                .ContainSingle(p => p.Key == expectedLogProperty)
                .Which.Value
                .Should()
                .BeOfType<ScalarValue>()
                .Which.Value
                .Should()
                .Be(CorrelationId);
        }

        _correlationIdFactory.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
    }

    [Fact]
    public void When_correlating_has_started_it_should_have_added_activity_to_httpContext_items()
    {
        // Act
        _sut.StartCorrelating(_httpContext);

        // Assert
        _httpContext.Items.Should()
            .ContainKey(CorrelateFeature.RequestActivityKey)
            .WhoseValue
            .Should()
            .BeSameAs(_activityMock);
        _correlationIdFactory.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
        _activityMock.DidNotReceive().Stop();
    }

    [Fact]
    public void When_correlating_has_stopped_it_should_have_stopped_activity()
    {
        _sut.StartCorrelating(_httpContext);

        // Act
        _sut.StopCorrelating(_httpContext);

        // Assert
        _httpContext.Items.Should()
            .ContainKey(CorrelateFeature.RequestActivityKey)
            .WhoseValue
            .Should()
            .BeSameAs(_activityMock);
        _correlationIdFactory.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
        _activityMock.Received(1).Stop();
    }

    [Fact]
    public async Task When_response_header_should_not_be_included_and_context_has_started_response_should_not_contain_header()
    {
        _options.IncludeInResponse = false;
        _options.RequestHeaders.Should().NotBeNullOrEmpty();

        // Act
        _sut.StartCorrelating(_httpContext);
        await _responseFeature.FireOnSendingHeadersAsync();

        // Assert
        _httpContext.Response.Headers.Should().BeEmpty();
        _correlationIdFactory.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(Arg.Any<string>());
    }
}
