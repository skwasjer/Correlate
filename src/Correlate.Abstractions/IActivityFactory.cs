namespace Correlate;

/// <summary>
/// Represents a factory that creates new activities, used to start a new correlation context.
/// </summary>
public interface IActivityFactory
{
    /// <summary>
    /// Creates a new activity that can be started and stopped manually.
    /// </summary>
    /// <returns>The correlated activity.</returns>
    IActivity CreateActivity();

    /// <summary>
    /// Starts a new activity with the specified correlation ID and returns the correlation context.
    /// </summary>
    /// <param name="correlationId">The correlation ID to use for the activity.</param>
    /// <param name="activity">The activity to start.</param>
    /// <returns>The correlation context associated with the started activity.</returns>
    CorrelationContext StartActivity(string? correlationId, IActivity activity);
}
