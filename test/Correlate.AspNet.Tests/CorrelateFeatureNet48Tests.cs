using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Correlate.AspNet.Middlewares;
using Correlate.AspNet.Options;
using Correlate.Http;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Xunit;

namespace Correlate.AspNet.Tests;

public class CorrelateFeatureNet48Tests : IDisposable
{
    private readonly HttpContextBase _httpContext;
    
    private readonly CorrelateFeatureNet48 _sut;
    private readonly IActivity _activityMock;
    private readonly IActivityFactory _activityFactoryMock;
    private readonly CorrelateOptionsNet48 _options;
    private readonly ICorrelationIdFactory _correlationIdFactory;
    private readonly ServiceProvider _services;
    private readonly FakeLogCollector _logCollector;

    private static readonly string CorrelationId = Guid.NewGuid().ToString("D");

    public CorrelateFeatureNet48Tests()
    {
        _httpContext = Substitute.For<HttpContextBase>();
        _httpContext.Request.Returns(Substitute.For<HttpRequestBase>());
        _httpContext.Request.Headers.Returns(new NameValueCollection());

        _httpContext.Response.Returns(Substitute.For<HttpResponseBase>());
        _httpContext.Response.Headers.Returns(new NameValueCollection());
        
        _httpContext.Items.Returns(new Dictionary<string, object>());
        
        _services = new ServiceCollection()
            .AddLogging(
                builder => builder
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFakeLogging()
                    .AddDebug())
            .BuildServiceProvider();
        
        _activityMock = Substitute.For<IActivity>();
        _activityMock.Start(Arg.Any<string>()).Returns(new CorrelationContext { CorrelationId = CorrelationId });
        
        _activityFactoryMock = Substitute.For<IActivityFactory>();
        _activityFactoryMock.CreateActivity().Returns(_activityMock);

        _correlationIdFactory = Substitute.For<ICorrelationIdFactory>();
        _correlationIdFactory.Create().Returns(CorrelationId);

        _options = new CorrelateOptionsNet48 { RequestHeaders = [CorrelationHttpHeaders.CorrelationId] };
        _sut = new CorrelateFeatureNet48(
            _services.GetRequiredService<ILoggerFactory>(),
            _correlationIdFactory,
            _activityFactoryMock,
            new OptionsWrapper<CorrelateOptionsNet48>(_options));
        
        _logCollector = _services.GetFakeLogCollector();
    }
    
    public void Dispose()
    {
        _services.Dispose();
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
        _sut.StartCorrelating(_httpContext);
        _sut.StopCorrelating(_httpContext);

        // Assert
        _httpContext.Response.Headers.Keys.Cast<string>().Should().Contain(expectedHeader.Key);
        _httpContext.Response.Headers[expectedHeader.Key].Should().Contain(expectedHeader.Value);
        _correlationIdFactory.Received(1).Create();
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
        _httpContext.Request.Headers.Add(headerName, correlationId);

        var expectedHeader = new KeyValuePair<string, StringValues>(
            headerName,
            correlationId
        );

        // Act
        _sut.StartCorrelating(_httpContext);
        _sut.StopCorrelating(_httpContext);

        // Assert
        _httpContext.Response.Headers.Keys.Cast<string>().Should().Contain(expectedHeader.Key);
        _httpContext.Response.Headers[expectedHeader.Key].Should().Contain(expectedHeader.Value);
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
        IOptions<CorrelateOptionsNet48> options,
        string expectedParamName,
        Type expectedExceptionType
    )
    {
        // Act
        Func<CorrelateFeatureNet48> act = () => new CorrelateFeatureNet48(loggerFactory, correlationIdFactory, activityFactory, options);

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
        IOptions<CorrelateOptionsNet48> options = Substitute.For<IOptions<CorrelateOptionsNet48>>();

        yield return [null, correlationIdFactory, activityFactory, options, nameof(loggerFactory), typeof(ArgumentNullException)];
        yield return [loggerFactory, null, activityFactory, options, nameof(correlationIdFactory), typeof(ArgumentNullException)];
        yield return [loggerFactory, correlationIdFactory, null, options, nameof(activityFactory), typeof(ArgumentNullException)];
        yield return [loggerFactory, correlationIdFactory, activityFactory, null, nameof(options), typeof(ArgumentException)];
    }

    [Fact]
    public void Given_that_activity_in_items_was_replaced_with_something_else_when_correlating_has_stopped_it_should_not_throw()
    {
        _sut.StartCorrelating(_httpContext);
        _httpContext.Items.Keys.Cast<string>().Should().Contain(CorrelateFeatureNet48.RequestActivityKey);
        _httpContext.Items[CorrelateFeatureNet48.RequestActivityKey] = new object();

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
        _httpContext.Items.Keys.Cast<string>().Should().Contain(CorrelateFeatureNet48.RequestActivityKey);
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
    public void Given_that_correlating_has_not_started_when_firing_to_send_headers_it_should_not_add_correlationId_header_to_response()
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();

        // Act

        // Assert
        _httpContext.Response.Headers.Keys.Cast<string>().Should().BeEmpty();
    }

    [Fact]
    public void Given_that_response_already_contains_correlation_header_when_firing_to_send_headers_it_should_not_overwrite_the_correlationId_header()
    {
        _options.IncludeInResponse.Should().BeTrue();
        _options.RequestHeaders.Should().NotBeNullOrEmpty();

        const string existingCorrelationId = "existing-id";
        _httpContext.Request.Headers.Add(_options.RequestHeaders![0], existingCorrelationId);

        var expectedHeader = new KeyValuePair<string, StringValues>(
            _options.RequestHeaders[0],
            existingCorrelationId
        );

        // Act
        _sut.StartCorrelating(_httpContext);
        _sut.StopCorrelating(_httpContext);

        // Assert
        _httpContext.Response.Headers.Keys.Cast<string>().Should().Contain(expectedHeader.Key);
        _httpContext.Response.Headers[expectedHeader.Key].Should().Contain(expectedHeader.Value);
        _correlationIdFactory.Received(0).Create(); // No new correlation ID should be created, this is diverging from the original behavior in CorrelateFeature
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
        using FakeLogContext context = _services.CreateLoggerContext();

        // Act
        _sut.StartCorrelating(_httpContext);
        _sut.StopCorrelating(_httpContext);

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
        _httpContext.Items.Keys.Cast<string>().ToDictionary(key => key, value => _httpContext.Items[value]).Should()
            .ContainKey(CorrelateFeatureNet48.RequestActivityKey)
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
        _httpContext.Items.Keys.Cast<string>().ToDictionary(key => key, value => _httpContext.Items[value]).Should()
            .ContainKey(CorrelateFeatureNet48.RequestActivityKey)
            .WhoseValue
            .Should()
            .BeSameAs(_activityMock);
        _correlationIdFactory.Received(1).Create();
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
        _sut.StartCorrelating(_httpContext);

        // Assert
        _httpContext.Response.Headers.Keys.Cast<string>().Should().BeEmpty();
        _correlationIdFactory.Received(1).Create();
        _activityFactoryMock.Received(1).CreateActivity();
        _activityMock.Received(1).Start(Arg.Any<string>());
    }
}
