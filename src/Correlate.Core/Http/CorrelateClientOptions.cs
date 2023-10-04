namespace Correlate.Http;

/// <summary>
/// Client options for adding correlation id to outgoing requests.
/// </summary>
public class CorrelateClientOptions
{
    /// <summary>
    /// Gets or sets the request header to set the correlation id in for outgoing requests. Default 'X-Correlation-ID'.
    /// </summary>
    public string RequestHeader { get; set; } = CorrelationHttpHeaders.CorrelationId;
}
