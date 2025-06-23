using System.Web;
using Correlate.AspNet.Extensions;
using Correlate.AspNet.Options;
using Correlate.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Correlate.AspNet.Middlewares;

/// <summary>
/// Implementation of Correlate feature for .NET Framework 4.8.
/// This will mimic the behavior of the Correlate feature in .NET 8+.<br/>
/// See <see cref="Correlate.AspNetCore.CorrelateFeature"/> for more details.
/// </summary>
public class CorrelateFeatureNet48 : ICorrelateFeatureNet48
{
    private const string LoggerCategory = "Correlate.AspNet";

    private static readonly Action<ILogger, string, string, Exception?> LogRequestHeaderFound =
        LoggerMessage.Define<string, string>(LogLevel.Trace, 0xC000, "Request header '{HeaderName}' found with correlation id '{CorrelationId}'.");

    private static readonly Action<ILogger, string, string, Exception?> LogResponseHeaderAdded =
        LoggerMessage.Define<string, string>(LogLevel.Trace, 0xC001, "Setting response header '{HeaderName}' to correlation id '{CorrelationId}'.");

    internal static readonly string RequestActivityKey = $"{typeof(CorrelateFeatureNet48).FullName}, {nameof(RequestActivityKey)}";
    internal static readonly string CorrelationContextKey = $"{typeof(CorrelateFeatureNet48).FullName}, {nameof(CorrelationContextKey)}";

    private readonly IActivityFactory _activityFactory;
    private readonly ICorrelationIdFactory _correlationIdFactory;
    private readonly ILogger _logger;
    private readonly CorrelateOptionsNet48 _options;
    
    public CorrelateFeatureNet48
    (
        ILoggerFactory loggerFactory,
        ICorrelationIdFactory correlationIdFactory,
        IActivityFactory activityFactory,
        IOptions<CorrelateOptionsNet48> options
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
    
    public void StartCorrelating(HttpContextBase httpContext)
    {
        Tuple<string?, string> correlationContext =
            GetOrCreateCorrelationHeaderAndId(httpContext);

        IActivity activity = _activityFactory.CreateActivity();
        activity.Start(correlationContext.Item2);
        // Save the activity so we can clean it up later.
        httpContext.Items[RequestActivityKey] = activity;
        httpContext.Items[CorrelationContextKey] = correlationContext;
    }

    public void StopCorrelating(HttpContextBase httpContext)
    {
        // ReSharper disable once InvertIf
        if (httpContext.Items.TryGetValue(RequestActivityKey, out object? activityObj) &&
            activityObj is IActivity activity)
        {
            if (httpContext.Items.TryGetValue(CorrelationContextKey, out object? correlationContextObj) &&
                correlationContextObj is Tuple<string?, string> correlationContext)
            {
                activity.Stop();

                // If already set, ignore.
                if (httpContext.Response.Headers.TryAdd(correlationContext.Item1!, correlationContext.Item2))
                {
                    LogResponseHeaderAdded(_logger, correlationContext.Item1!, correlationContext.Item2, null);
                }
            }
        }
    }

    private Tuple<string?, string> GetOrCreateCorrelationHeaderAndId(HttpContextBase httpContext)
    {
        KeyValuePair<string, string?> keyValuePair = httpContext.Request.Headers.GetCorrelationIdHeader(_options.RequestHeaders ?? [CorrelationHttpHeaders.CorrelationId]);
        string requestHeaderName = keyValuePair.Key;
        string? requestCorrelationId = keyValuePair.Value;

        if (requestCorrelationId is not null)
        {
            LogRequestHeaderFound(_logger, requestHeaderName, requestCorrelationId, null);
        }

        return new Tuple<string?, string>(
            _options.IncludeInResponse
                ? requestHeaderName
                : null,
            requestCorrelationId ?? _correlationIdFactory.Create()
            );
    }
}
