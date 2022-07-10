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
}
