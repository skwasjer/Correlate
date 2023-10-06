using Correlate.Http;

namespace Correlate.AspNetCore;

/// <summary>
/// Options for handling correlation id on incoming requests.
/// </summary>
public sealed class CorrelateOptions: CorrelationManagerOptions
{
    private static readonly string[] DefaultRequestHeaders = { CorrelationHttpHeaders.CorrelationId };

    private string[]? _requestHeaders;

    /// <summary>
    /// Gets or sets the request headers to retrieve the correlation id from. Defaults to <c>X-Correlation-ID</c>.
    /// </summary>
    /// <remarks>
    /// The first matching request header will be used.
    /// </remarks>
    public string[] RequestHeaders
    {
        get => _requestHeaders ?? DefaultRequestHeaders;
        set => _requestHeaders = value;
    }

    /// <summary>
    /// Gets or sets whether to include the correlation id in the response.
    /// <para>If the correlation id was received in the request, it will use the exact same header name in the response. If the request did not have a correlation id header (matching one in <see cref="RequestHeaders" />), the default response header will be <c>X-Correlation-ID</c>.</para>
    /// </summary>
    /// <remarks>
    /// You may want to consider disabling this in edge services, so that tracing details are not exposed to the outside world.
    /// </remarks>
    public bool IncludeInResponse { get; set; } = true;
}
