using Correlate.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.TestCorrelator;
using Xunit;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Correlate.AspNetCore.Middleware;

public class HttpRequestActivityTests : IDisposable
{
    private readonly DefaultHttpContext _httpContext;
    private readonly HttpRequestActivity _sut;
    private readonly CorrelationContext _correlationContext;
    private readonly TestResponseFeature _responseFeature;
    private readonly ILogger _logger;

    public HttpRequestActivityTests()
    {
        _httpContext = new DefaultHttpContext();
        _httpContext.Features.Set<IHttpResponseFeature>(_responseFeature = new TestResponseFeature());

        Logger serilog = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestCorrelator()
            .CreateLogger();

        var serilogProvider = new SerilogLoggerProvider(serilog);
        _logger = serilogProvider.CreateLogger("");

        _sut = new HttpRequestActivity(_logger, _httpContext, CorrelationHttpHeaders.CorrelationId);
        _correlationContext = new CorrelationContext { CorrelationId = Guid.NewGuid().ToString() };
    }

    public void Dispose()
    {
        _sut.Stop();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task When_context_has_not_started_response_should_not_contain_header()
    {
        // Act
        await _responseFeature.FireOnSendingHeadersAsync();

        // Assert
        _httpContext.Response.Headers.Should().BeEmpty();
    }

    [Fact]
    public async Task When_context_has_started_response_should_contain_header()
    {
        var expectedHeader = new KeyValuePair<string, StringValues>(
            CorrelationHttpHeaders.CorrelationId,
            new StringValues(_correlationContext.CorrelationId)
        );

        // Act
        _sut.Start(_correlationContext);
        await _responseFeature.FireOnSendingHeadersAsync();

        // Assert
        _httpContext.Response.Headers.Should().Contain(expectedHeader);
    }

    [Fact]
    public async Task When_context_has_started_should_create_logScope_with_correlationId()
    {
        const string expectedLogProperty = CorrelateConstants.CorrelationIdKey;

        // Act
        using (TestCorrelator.CreateContext())
        {
            _sut.Start(_correlationContext);
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
                .Be(_correlationContext.CorrelationId);
        }
    }

    [Fact]
    public async Task When_no_header_name_is_provided_and_context_has_started_response_should_not_contain_header()
    {
        var sut = new HttpRequestActivity(_logger, _httpContext, null);

        // Act
        sut.Start(_correlationContext);
        await _responseFeature.FireOnSendingHeadersAsync();

        // Assert
        _httpContext.Response.Headers.Should().BeEmpty();
    }
}
