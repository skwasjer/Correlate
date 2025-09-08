using System.Diagnostics;

namespace Correlate.AspNet.Tests;

/// <summary>
/// Marker to disable parallel tests because it uses static <see cref="DiagnosticListener"/>.
/// </summary>
[CollectionDefinition(nameof(UsesDiagnosticListener), DisableParallelization = true)]
public class UsesDiagnosticListener
{
}
