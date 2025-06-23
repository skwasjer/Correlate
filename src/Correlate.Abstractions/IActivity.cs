namespace Correlate;

/// <summary>
/// Describes an activity that can be started and stopped as desired to signal start/stop of a correlation context.
/// </summary>
public interface IActivity
{
    /// <summary>
    /// Signals the start of an the activity
    /// </summary>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <returns>The created correlation context (also accessible via <see cref="ICorrelationContextAccessor" />), or null if diagnostics and logging is disabled.</returns>
    CorrelationContext Start(string correlationId);

    /// <summary>
    /// Signals the activity is complete.
    /// </summary>
#pragma warning disable CA1716
    void Stop();
#pragma warning restore CA1716
}
