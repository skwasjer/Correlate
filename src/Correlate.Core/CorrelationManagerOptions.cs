namespace Correlate;

/// <summary>
/// The options class for configuring <see cref="CorrelationManager"/>.
/// </summary>
public class CorrelationManagerOptions
{
    /// <summary>
    /// The scope key that will be used for adding correlation ID to log context. Default is <c>CorrelationId</c>.
    /// </summary>
    public string LoggingScopeKey { get; set; } = CorrelateConstants.CorrelationIdKey;
}
