using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Correlate.AspNetCore.Diagnostics;

/// <summary>
/// Observer for ASP.NET Core. Registers the HttpRequestIn observer.
/// </summary>
internal sealed class AspNetDiagnosticsObserver : IObserver<DiagnosticListener>
{
    private const string AspNetCore = "Microsoft.AspNetCore";

    private readonly HttpRequestInDiagnosticsObserver _requestInDiagnosticsObserver;
    private readonly List<IDisposable> _subscriptions = new();

    public AspNetDiagnosticsObserver(ICorrelateFeature correlateFeature)
    {
        _requestInDiagnosticsObserver = new HttpRequestInDiagnosticsObserver(correlateFeature);
    }

    public void OnCompleted()
    {
        lock (_subscriptions)
        {
            _subscriptions.ForEach(x => x.Dispose());
            _subscriptions.Clear();
        }
    }

    [ExcludeFromCodeCoverage]
    public void OnError(Exception error)
    {
        // N/A
    }

    public void OnNext(DiagnosticListener value)
    {
        if (value.Name != AspNetCore)
        {
            return;
        }

        lock (_subscriptions)
        {
            IDisposable subscription = value.Subscribe(_requestInDiagnosticsObserver, HttpRequestInDiagnosticsObserver.IsEnabled);
            _subscriptions.Add(subscription);
        }
    }
}
