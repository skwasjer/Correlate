namespace Correlate.Http;

/// <summary>
/// Common correlation HTTP request headers.
/// </summary>
public static class CorrelationHttpHeaders
{
    /// <summary>
    /// The header 'X-Correlation-ID'.
    /// </summary>
    public const string CorrelationId = "X-Correlation-ID";

    /// <summary>
    /// The header 'X-Request-ID'. Not recommended (for possible future implementation of causation).
    /// </summary>
    public const string RequestId = "X-Request-ID";
}
