using Correlate.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Correlate.Http.Server;

/// <summary>
/// Represents the default implementation of an HTTP listener that handles correlation and activity tracking
/// for incoming HTTP requests.
/// </summary>
/// <remarks>
/// This class can be used by any library that wants to handle correlation and activity tracking for incoming HTTP requests. In itself it does not integrate with any specific web framework, but can be used to integrate via for example a diagnostics observer, as middleware component, or (HTTP) message handlers/pipelines, etc.
/// </remarks>
public sealed class DefaultHttpListener
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
    private readonly HttpListenerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultHttpListener"/> class.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> used to create a logger for logging operations.</param>
    /// <param name="correlationIdFactory">The <see cref="ICorrelationIdFactory"/> used to generate correlation IDs for requests.</param>
    /// <param name="activityFactory">The <see cref="IActivityFactory"/> used to create activities for tracking request processing.</param>
    /// <param name="options">The <see cref="IOptions{TOptions}"/> containing configuration settings for the HTTP listener.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="loggerFactory"/>, <paramref name="correlationIdFactory"/>, or <paramref name="activityFactory"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the <paramref name="loggerFactory"/> fails to create a logger instance.</exception>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="options"/> parameter contains a <c>null</c> value for its <c>Value</c> property.</exception>
    public DefaultHttpListener
    (
        ILoggerFactory loggerFactory,
        ICorrelationIdFactory correlationIdFactory,
        IActivityFactory activityFactory,
        IOptions<HttpListenerOptions> options
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

    /// <inheritdoc />
    public void HandleBeginRequest(IHttpListenerContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        (string? responseHeaderName, string?[]? headerValues, string correlationId) = GetOrCreateCorrelationHeaderAndId(context);

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
            if (context.TryAddResponseHeader(responseHeaderName, headerValues))
            {
                LogResponseHeaderAdded(_logger, responseHeaderName, correlationId, null);
            }
        });
    }

    /// <inheritdoc />
    public void HandleEndRequest(IHttpListenerContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.Items.TryGetValue(RequestActivityKey, out object? activityObj)
         && activityObj is IActivity activity)
        {
            activity.Stop();
        }
    }

    private (string? headerName, string?[]? headerValues, string correlationId) GetOrCreateCorrelationHeaderAndId(IHttpListenerContext httpContext)
    {
        KeyValuePair<string, string?[]?> kvp = httpContext.GetCorrelationIdHeader(_options.RequestHeaders ?? [CorrelationHttpHeaders.CorrelationId]);
        string headerName = kvp.Key;
        string?[]? headerValues = kvp.Value;
        string? correlationId;
        if (headerValues?.Length > 0)
        {
            correlationId = string.Join(",", headerValues);
            LogRequestHeaderFound(_logger, headerName, correlationId, null);
        }
        else
        {
            correlationId = _correlationIdFactory.Create();
            headerValues = [correlationId];
        }

        return _options.IncludeInResponse
            ? (headerName, headerValues, correlationId)
            : (null, null, correlationId);
    }
}
