namespace Correlate;

/// <summary>
/// Describes methods for starting a correlation context asynchronously.
/// </summary>
public interface ICorrelationManager
{
    /// <summary>
    /// Executes the <paramref name="correlatedAction" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <param name="correlatedAction">The action to execute.</param>
    /// <param name="onError">A delegate to handle the error inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the error handled, or <see langword="false" /> to throw.</param>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
    /// </remarks>
    public void Correlate(string? correlationId, Action correlatedAction, OnError? onError);

    /// <summary>
    /// Executes the <paramref name="correlatedFunc" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <param name="correlatedFunc">The func to execute.</param>
    /// <param name="onError">A delegate to handle the error inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the error handled, or <see langword="false" /> to throw.</param>
    /// <returns>Returns the result of the <paramref name="correlatedFunc" />.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
    /// </remarks>
    public T Correlate<T>(string? correlationId, Func<T> correlatedFunc, OnError<T>? onError);
}
