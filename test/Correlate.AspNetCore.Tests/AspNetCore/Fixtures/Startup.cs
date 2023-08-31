using System.Diagnostics;
using Correlate.AspNetCore.Diagnostics;
using Correlate.DependencyInjection;
using MockHttp;
using Serilog.Sinks.TestCorrelator;

namespace Correlate.AspNetCore.Fixtures;

public class Startup
{
    private static readonly TestCorrelatorObserver Observer = new();

    public static ITestCorrelatorContext? LastRequestContext => Observer.Current;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCorrelate(opts => opts.IncludeInResponse = true);

        services
            .AddHttpClient<TestController>(client => client.BaseAddress = new Uri("http://0.0.0.0"))
            .ConfigurePrimaryHttpMessageHandler(s => s.GetRequiredService<MockHttpHandler>())
            .CorrelateRequests();

        services
            .AddControllers()
            .AddControllersAsServices();
    }

    public void Configure(IApplicationBuilder app)
    {
        // Create context to track log events.
        DiagnosticListener diagnosticListener = app.ApplicationServices.GetRequiredService<DiagnosticListener>();
        IHostApplicationLifetime hostAppLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();

        IDisposable subscription = diagnosticListener.Subscribe(Observer, TestCorrelatorObserver.IsEnabled);
        hostAppLifetime.ApplicationStopping.Register(subscription.Dispose);

        app.UseCorrelate();

        app.UseRouting();
        app.UseEndpoints(builder => builder.MapControllers());
    }

    private sealed class TestCorrelatorObserver
        : IObserver<KeyValuePair<string, object?>>,
          IDisposable
    {
        public ITestCorrelatorContext? Current { get; private set; }

        public void Dispose()
        {
            Current?.Dispose();
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(KeyValuePair<string, object?> value)
        {
            Current?.Dispose();
            Current ??= TestCorrelator.CreateContext();
        }

        public static bool IsEnabled(string operationName)
        {
            return operationName is HttpRequestInDiagnosticsObserver.ActivityName
                or HttpRequestInDiagnosticsObserver.ActivityStartKey
                or HttpRequestInDiagnosticsObserver.ActivityStopKey;
        }
    }
}
