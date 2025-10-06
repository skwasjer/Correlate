using Correlate.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Correlate.Http.Server;

public sealed class DefaultHttpListenerTests : IDisposable
{
    private readonly DefaultHttpListener _sut;
    private readonly IActivity _activityMock;
    private readonly IActivityFactory _activityFactoryMock;
    private readonly HttpListenerOptions _options;
    private readonly ICorrelationIdFactory _correlationIdFactoryMock;
    private readonly FakeLogCollector _logCollector;
    private readonly HttpListenerContextStub _httpListenerContextStub;
    private readonly LoggerFactory _loggerFactory;

    private static readonly string CorrelationId = Guid.NewGuid().ToString("D");

    public DefaultHttpListenerTests()
    {
        _httpListenerContextStub = new HttpListenerContextStub();

        _activityMock = Substitute.For<IActivity>();
        _activityFactoryMock = Substitute.For<IActivityFactory>();
        _activityFactoryMock.CreateActivity().Returns(_activityMock);

        _correlationIdFactoryMock = Substitute.For<ICorrelationIdFactory>();
        _correlationIdFactoryMock.Create().Returns(CorrelationId);

        _logCollector = new FakeLogCollector();
        _loggerFactory = new LoggerFactory();
        _loggerFactory.AddProvider(new FakeLoggerProvider(_logCollector));

        _options = new HttpListenerOptions { RequestHeaders = [CorrelationHttpHeaders.CorrelationId] };
        _sut = new DefaultHttpListener(
            _loggerFactory,
            _correlationIdFactoryMock,
            _activityFactoryMock,
            Options.Create(_options));
    }

    public void Dispose()
    {
        _loggerFactory.Dispose();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("X-Custom-Header")]
    public void Given_that_correlating_has_started_when_firing_to_send_headers_it_should_add_correlationId_header_to_response(string? requestHeader)
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();
        if (requestHeader is not null)
        {
            _options.RequestHeaders = [requestHeader];
        }

        var expectedHeader = new KeyValuePair<string, StringValues>(
            _options.RequestHeaders![0],
            new StringValues(CorrelationId)
        );

        // Act
        _sut.HandleBeginRequest(_httpListenerContextStub);
        _httpListenerContextStub.FireOnStartingResponse();

        // Assert
        _httpListenerContextStub.ResponseHeaders.Should().Contain(expectedHeader);
        _correlationIdFactoryMock.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
    }

    [Theory]
    [InlineData(CorrelationHttpHeaders.CorrelationId)]
    [InlineData(CorrelationHttpHeaders.RequestId)]
    public void Given_that_request_contains_correlationId_header_in_allowed_list_when_correlating_has_started_it_should_have_used_that_correlationId(string headerName)
    {
        _options.RequestHeaders = [CorrelationHttpHeaders.CorrelationId, CorrelationHttpHeaders.RequestId];

        string correlationId = Guid.NewGuid().ToString("D");
        _httpListenerContextStub.RequestHeaders[headerName] = correlationId;

        var expectedHeader = new KeyValuePair<string, StringValues>(
            headerName,
            correlationId
        );

        // Act
        _sut.HandleBeginRequest(_httpListenerContextStub);
        _httpListenerContextStub.FireOnStartingResponse();

        // Assert
        _httpListenerContextStub.ResponseHeaders.Should().Contain(expectedHeader);
        _correlationIdFactoryMock.DidNotReceive().Create();
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
        object options,
        string expectedParamName,
        Type expectedExceptionType
    )
    {
        // Act
        Func<DefaultHttpListener> act = () => new DefaultHttpListener(loggerFactory, correlationIdFactory, activityFactory, (IOptions<HttpListenerOptions>)options);

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
        IOptions<HttpListenerOptions> options = Substitute.For<IOptions<HttpListenerOptions>>();

        yield return [null, correlationIdFactory, activityFactory, options, nameof(loggerFactory), typeof(ArgumentNullException)];
        yield return [loggerFactory, null, activityFactory, options, nameof(correlationIdFactory), typeof(ArgumentNullException)];
        yield return [loggerFactory, correlationIdFactory, null, options, nameof(activityFactory), typeof(ArgumentNullException)];
        yield return [loggerFactory, correlationIdFactory, activityFactory, null, nameof(options), typeof(ArgumentException)];
    }

    [Fact]
    public void Given_that_activity_in_items_was_replaced_with_something_else_when_correlating_has_stopped_it_should_not_throw()
    {
        _sut.HandleBeginRequest(_httpListenerContextStub);
        _httpListenerContextStub.Items.Should().ContainKey(DefaultHttpListener.RequestActivityKey);
        _httpListenerContextStub.Items[DefaultHttpListener.RequestActivityKey] = new object();

        // Act
        Action act = () => _sut.HandleEndRequest(_httpListenerContextStub);

        // Assert
        act.Should().NotThrow();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
        _activityMock.DidNotReceive().Stop();
    }

    [Fact]
    public void Given_that_activity_is_not_in_items_when_correlating_has_stopped_it_should_not_throw()
    {
        _sut.HandleBeginRequest(_httpListenerContextStub);
        _httpListenerContextStub.Items.Should().ContainKey(DefaultHttpListener.RequestActivityKey);
        _httpListenerContextStub.Items.Clear();

        // Act
        Action act = () => _sut.HandleEndRequest(_httpListenerContextStub);

        // Assert
        act.Should().NotThrow();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
        _activityMock.DidNotReceive().Stop();
    }

    [Fact]
    public void Given_that_correlating_has_not_started_when_firing_to_send_headers_it_should_not_add_correlationId_header_to_response()
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();

        // Act
        _httpListenerContextStub.FireOnStartingResponse();

        // Assert
        _httpListenerContextStub.ResponseHeaders.Should().BeEmpty();
    }

    [Fact]
    public void Given_that_response_already_contains_correlation_header_when_firing_to_send_headers_it_should_not_overwrite_the_correlationId_header()
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();

        const string existingCorrelationId = "existing-id";
        _httpListenerContextStub.ResponseHeaders[_options.RequestHeaders![0]] = existingCorrelationId;

        var expectedHeader = new KeyValuePair<string, StringValues>(
            _options.RequestHeaders[0],
            existingCorrelationId
        );

        // Act
        _sut.HandleBeginRequest(_httpListenerContextStub);
        _httpListenerContextStub.FireOnStartingResponse();

        // Assert
        _httpListenerContextStub.ResponseHeaders.Should().Contain(expectedHeader);
        _correlationIdFactoryMock.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(Arg.Any<string>());
    }

    [Fact]
    public void When_correlating_has_started_it_should_create_logScope_with_correlationId()
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();
        const string expectedLogProperty = CorrelateConstants.CorrelationIdKey;

        _logCollector.Clear();
        using FakeLogContext context = _loggerFactory.CreateLogger("Correlate.AspNetCore").CreateLoggerContext();

        // Act
        _sut.HandleBeginRequest(_httpListenerContextStub);
        _httpListenerContextStub.FireOnStartingResponse();

        // Assert
        IReadOnlyList<FakeLogRecord> logEvents = _logCollector.GetSnapshot(context, true);
        logEvents
            .Should()
            .ContainSingle(le => le.Message.StartsWith("Setting response header"))
            .Which.StructuredState
            .Should()
            // this tests the {CorrelationId} from log message template in CorrelateFeature.LogRequestHeaderFound, not the one from log scope added by IActivityFactory.CreateActivity
            .ContainKey(expectedLogProperty)
            .WhoseValue.Should()
            .Be(CorrelationId);

        _correlationIdFactoryMock.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
    }

    [Fact]
    public void When_correlating_has_started_it_should_have_added_activity_to_httpContext_items()
    {
        // Act
        _sut.HandleBeginRequest(_httpListenerContextStub);

        // Assert
        _httpListenerContextStub.Items.Should()
            .ContainKey(DefaultHttpListener.RequestActivityKey)
            .WhoseValue
            .Should()
            .BeSameAs(_activityMock);
        _correlationIdFactoryMock.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
        _activityMock.DidNotReceive().Stop();
    }

    [Fact]
    public void When_correlating_has_stopped_it_should_have_stopped_activity()
    {
        _sut.HandleBeginRequest(_httpListenerContextStub);

        // Act
        _sut.HandleEndRequest(_httpListenerContextStub);

        // Assert
        _httpListenerContextStub.Items.Should()
            .ContainKey(DefaultHttpListener.RequestActivityKey)
            .WhoseValue
            .Should()
            .BeSameAs(_activityMock);
        _correlationIdFactoryMock.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(CorrelationId);
        _activityMock.Received(1).Stop();
    }

    [Fact]
    public void When_response_header_should_not_be_included_and_context_has_started_response_should_not_contain_header()
    {
        _options.IncludeInResponse = false;
        _options.RequestHeaders.Should().NotBeNullOrEmpty();

        // Act
        _sut.HandleBeginRequest(_httpListenerContextStub);
        _httpListenerContextStub.FireOnStartingResponse();

        // Assert
        _httpListenerContextStub.ResponseHeaders.Should().BeEmpty();
        _correlationIdFactoryMock.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(Arg.Any<string>());
    }
}
