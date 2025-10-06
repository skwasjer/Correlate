using Correlate.Http;
using Correlate.Http.Extensions;
using Correlate.Http.Server;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Correlate.AspNetCore;

internal sealed class DefaultHttpListener
    : IHttpListener
{
    // Previously (pre v5), our log messages were emitted with Correlate.AspNetCore.Middleware category
    // because we depended on a middleware implementation which got injected an ILogger<>.
    // But we don't have middleware anymore so that old category is misleading. In case people want
    // to filter out the log messages we should ensure the category is pinned so we don't introduce
    // a breaking change again in the future.
    private const string LoggerCategory = "Correlate.AspNetCore";

    private static readonly Action<ILogger, string, string, Exception?> LogRequestHeaderFound =
        LoggerMessage.Define<string, string>(LogLevel.Trace, 0xC000, "Request header '{HeaderName}' found with correlation id '{CorrelationId}'.");

    private static readonly Action<ILogger, string, string, Exception?> LogResponseHeaderAdded =
        LoggerMessage.Define<string, string>(LogLevel.Trace, 0xC001, "Setting response header '{HeaderName}' to correlation id '{CorrelationId}'.");

    internal static readonly string RequestActivityKey = $"{typeof(DefaultHttpListener).FullName}, {nameof(RequestActivityKey)}";

    private readonly IActivityFactory _activityFactory;
    private readonly ICorrelationIdFactory _correlationIdFactory;
    private readonly ILogger _logger;
    private readonly CorrelateOptions _options;

    public DefaultHttpListener
    (
        ILoggerFactory loggerFactory,
        ICorrelationIdFactory correlationIdFactory,
        IActivityFactory activityFactory,
        IOptions<CorrelateOptions> options
    )
    {
        if (loggerFactory is null)
        {
            throw new ArgumentNullException(nameof(loggerFactory));
        }

        // Even though by contract, we should never have nulls, check anyway. Last thing we want is
        // for people to experience NRE's in the ASP.NET pipeline and blaming me :)
        _logger = loggerFactory.CreateLogger(LoggerCategory) ?? throw new InvalidOperationException("The logger factory did not create a logger instance.");
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
        _activityFactory = activityFactory ?? throw new ArgumentNullException(nameof(activityFactory));
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _options = options?.Value ?? throw new ArgumentException("The 'Value' returns null.", nameof(options));
    }

    public void HandleBeginRequest(IHttpListenerContext context)
    {
        (string? responseHeaderName, string correlationId) = GetOrCreateCorrelationHeaderAndId(context);

        IActivity activity = _activityFactory.CreateActivity();
        activity.Start(correlationId);
        // Save the activity so we can clean it up later.
        context.Items[RequestActivityKey] = activity;

        // No response header needs to be attached, so done.
        if (responseHeaderName is null)
        {
            return;
        }

        context.OnStartingResponse(() =>
        {
            // If already set, ignore.
            if (context.TryAddResponseHeader(responseHeaderName, correlationId))
            {
                LogResponseHeaderAdded(_logger, responseHeaderName, correlationId, null);
            }
        });
    }

    public void HandleEndRequest(IHttpListenerContext context)
    {
        if (context.Items.TryGetValue(RequestActivityKey, out object? activityObj)
         && activityObj is IActivity activity)
        {
            activity.Stop();
        }
    }

    private (string? headerName, string correlationId) GetOrCreateCorrelationHeaderAndId(IHttpListenerContext httpContext)
    {
        (string requestHeaderName, string? requestCorrelationId) = httpContext.GetCorrelationIdHeader(_options.RequestHeaders ?? [CorrelationHttpHeaders.CorrelationId]);
        if (requestCorrelationId is not null)
        {
            LogRequestHeaderFound(_logger, requestHeaderName, requestCorrelationId, null);
        }

        return (
            _options.IncludeInResponse
                ? requestHeaderName
                : null,
            requestCorrelationId ?? _correlationIdFactory.Create()
            );
    }
}
