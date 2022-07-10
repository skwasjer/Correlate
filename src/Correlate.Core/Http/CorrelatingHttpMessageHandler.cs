using Microsoft.Extensions.Options;

namespace Correlate.Http;

/// <summary>
/// Enriches the outgoing request with a correlation id header provided by <see cref="ICorrelationContextAccessor" />.
/// </summary>
public class CorrelatingHttpMessageHandler : DelegatingHandler
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;
    private readonly CorrelateClientOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelatingHttpMessageHandler" /> class using specified context accessor.
    /// </summary>
    /// <param name="correlationContextAccessor">The correlation context accessor.</param>
    /// <param name="options">The client correlation options.</param>
    public CorrelatingHttpMessageHandler(ICorrelationContextAccessor correlationContextAccessor, IOptions<CorrelateClientOptions> options)
    {
        _correlationContextAccessor = correlationContextAccessor;
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _options = options?.Value ?? new CorrelateClientOptions();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelatingHttpMessageHandler" /> class using specified context accessor.
    /// </summary>
    /// <param name="correlationContextAccessor">The correlation context accessor.</param>
    /// <param name="options">The client correlation options.</param>
    /// <param name="innerHandler">The inner handler.</param>
    public CorrelatingHttpMessageHandler(ICorrelationContextAccessor correlationContextAccessor, IOptions<CorrelateClientOptions> options, HttpMessageHandler innerHandler)
        : this(correlationContextAccessor, options)
    {
        InnerHandler = innerHandler;
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        string? correlationId = _correlationContextAccessor?.CorrelationContext?.CorrelationId;
        if (correlationId is not null && !request.Headers.Contains(_options.RequestHeader))
        {
            request.Headers.TryAddWithoutValidation(_options.RequestHeader, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
