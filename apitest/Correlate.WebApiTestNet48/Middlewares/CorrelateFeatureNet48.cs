using System;
using System.Collections.Generic;
using System.Web;
using Correlate.DependencyInjection;
using Correlate.Http;
using Correlate.WebApiTestNet48.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Correlate.WebApiTestNet48.Middlewares;

/// <summary>
/// Implementation of Correlate feature for .NET Framework 4.8.
/// This wihll micmic the behavior of the Correlate feature in .NET 8+.
/// See <see cref="Correlate.AspNetCore.CorrelateFeature"/> for more details.
/// </summary>
public class CorrelateFeatureNet48 : ICorrelateFeatureNet48
{
    private const string LoggerCategory = "Correlate.AspNetCore";

    private static readonly Action<ILogger, string, string, Exception?> LogRequestHeaderFound =
        LoggerMessage.Define<string, string>(LogLevel.Trace, 0xC000, "Request header '{HeaderName}' found with correlation id '{CorrelationId}'.");

    private static readonly Action<ILogger, string, string, Exception?> LogResponseHeaderAdded =
        LoggerMessage.Define<string, string>(LogLevel.Trace, 0xC001, "Setting response header '{HeaderName}' to correlation id '{CorrelationId}'.");

    internal static readonly string RequestActivityKey = $"{typeof(CorrelateFeatureNet48).FullName}, {nameof(RequestActivityKey)}";

    private readonly IActivityFactory _activityFactory;
    private readonly ICorrelationIdFactory _correlationIdFactory;
    private readonly ILogger _logger;
    private readonly CorrelateOptions _options;
    
    public CorrelateFeatureNet48
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
    
    public void StartCorrelating(HttpContext httpContext)
    {
        (string? _, string correlationId) = GetOrCreateCorrelationHeaderAndId(httpContext);

        IActivity activity = _activityFactory.CreateActivity();
        activity.Start(correlationId);
        // Save the activity so we can clean it up later.
        httpContext.Items[RequestActivityKey] = activity;
    }

    public void StopCorrelating(HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(RequestActivityKey, out object? activityObj)
         && activityObj is IActivity activity)
        {
            activity.Stop();
            (string? responseHeaderName, string correlationId) = GetOrCreateCorrelationHeaderAndId(httpContext);

            // If already set, ignore.
            if (httpContext.Response.Headers.TryAdd(responseHeaderName, correlationId))
            {
                LogResponseHeaderAdded(_logger, responseHeaderName, correlationId, null);
            }

        }
        
    }

    private (string? headerName, string correlationId) GetOrCreateCorrelationHeaderAndId(HttpContext httpContext)
    {
        KeyValuePair<string, string?> keyValuePair = httpContext.Request.Headers.GetCorrelationIdHeader(_options.RequestHeaders ?? [CorrelationHttpHeaders.CorrelationId]);
        string requestHeaderName = keyValuePair.Key;
        string? requestCorrelationId = keyValuePair.Value;

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
