using System.Collections.Immutable;
using Correlate.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Correlate.AspNetCore.Middleware;

/// <summary>
/// Middleware that takes correlation id from incoming request header and persists it throughout the request chain in a <see cref="CorrelationContext" /> object.
/// </summary>
public class CorrelateMiddleware
{
    private readonly ImmutableArray<string> _acceptedRequestHeaders;
    private readonly IAsyncCorrelationManager _asyncCorrelationManager;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;
    private readonly ILogger<CorrelateMiddleware> _logger;
    private readonly RequestDelegate _next;
    private readonly CorrelateOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelateMiddleware" /> class.
    /// </summary>
    /// <param name="next">The next request delegate to invoke in the request execution pipeline.</param>
    /// <param name="options">The options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="correlationContextAccessor">The correlation context accessor.</param>
    /// <param name="asyncCorrelationManager">The correlation manager.</param>
    public CorrelateMiddleware
    (
        RequestDelegate next,
        IOptions<CorrelateOptions> options,
        ILogger<CorrelateMiddleware> logger,
        ICorrelationContextAccessor correlationContextAccessor,
        IAsyncCorrelationManager asyncCorrelationManager)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
        _asyncCorrelationManager = asyncCorrelationManager ?? throw new ArgumentNullException(nameof(asyncCorrelationManager));

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_options.RequestHeaders is null || !_options.RequestHeaders.Any())
        {
            _acceptedRequestHeaders = ImmutableArray<string>.Empty;
        }
        else
        {
            _acceptedRequestHeaders = _options.RequestHeaders.ToImmutableArray();
        }
    }

    /// <summary>
    /// Invokes the middleware for the current <paramref name="httpContext" />.
    /// </summary>
    /// <param name="httpContext">The current <see cref="HttpContext" />.</param>
    /// <returns>An awaitable to wait for to complete the request.</returns>
    public Task Invoke(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        // ReSharper disable once UseDeconstruction - not supported in .NET46/.NETS
        KeyValuePair<string, string?> requestHeader = httpContext.Request.Headers.GetCorrelationIdHeader(_acceptedRequestHeaders);
        if (requestHeader.Value is not null)
        {
            _logger.LogTrace("Request header '{HeaderName}' found with correlation id '{CorrelationId}'.", requestHeader.Key, requestHeader.Value);
        }

        string? responseHeaderName = _options.IncludeInResponse ? requestHeader.Key : null;
        var correlatedHttpRequest = new HttpRequestActivity(_logger, httpContext, responseHeaderName);
        return _asyncCorrelationManager.CorrelateAsync(
            requestHeader.Value,
            () =>
            {
                correlatedHttpRequest.Start(_correlationContextAccessor.CorrelationContext);
                return _next(httpContext)
                    .ContinueWith(t =>
                        {
                            correlatedHttpRequest.Stop();
                            return t;
                        },
                        TaskScheduler.Current)
                    .Unwrap();
            });
    }
}
