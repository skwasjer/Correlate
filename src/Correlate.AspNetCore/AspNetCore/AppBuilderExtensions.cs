using System.Diagnostics;
using Correlate.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Correlate.AspNetCore;

/// <summary>
/// Registration extensions for <see cref="IApplicationBuilder" />.
/// </summary>
public static class AppBuilderExtensions
{
    /// <summary>
    /// Adds Correlate middleware to the request execution pipeline.
    /// </summary>
    /// <param name="appBuilder">The <see cref="IApplicationBuilder" />.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IApplicationBuilder UseCorrelate(this IApplicationBuilder appBuilder)
    {
        if (appBuilder is null)
        {
            throw new ArgumentNullException(nameof(appBuilder));
        }

        if (appBuilder.ServerFeatures.Get<ICorrelateFeature>() is not null)
        {
            throw new InvalidOperationException($"{nameof(UseCorrelate)}() should not be called more than once.");
        }

        // Register HttpRequestIn observer.
        CorrelateFeature correlateFeature = ActivatorUtilities.CreateInstance<CorrelateFeature>(appBuilder.ApplicationServices);
        appBuilder.ServerFeatures.Set<ICorrelateFeature>(correlateFeature);
        IDisposable observerDisposable = DiagnosticListener.AllListeners.Subscribe(new AspNetDiagnosticsObserver(correlateFeature));
        // Dispose the observer on app shutdown.
        IHostApplicationLifetime applicationLifetime = appBuilder.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        applicationLifetime.ApplicationStopping.Register(OnShutdown, observerDisposable);

        return appBuilder;
    }

    private static void OnShutdown(object? service)
    {
        (service as IDisposable)?.Dispose();
    }
}
