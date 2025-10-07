namespace Correlate;

/// <summary>
/// Marker to disable parallel tests (eg. when setting static singletons in context of specific tests).
/// </summary>
[CollectionDefinition(nameof(DisableParallelization), DisableParallelization = true)]
public class DisableParallelization
{
}
