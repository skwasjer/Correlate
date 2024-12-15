using System.Diagnostics;
using System.Runtime.CompilerServices;
using Correlate.AspNetCore.Diagnostics;
using Correlate.DependencyInjection;
using Correlate.Testing;
using MockHttp;

namespace Correlate.AspNetCore.Fixtures;

public class Startup
{
    private static readonly TestCorrelatorObserver Observer = new();

    public static FakeLogContext? LastRequestContext { get => Observer.Current; }

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
        public FakeLogContext? Current { get; private set; }

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
            if (value.Key == HttpRequestInDiagnosticsObserver.ActivityStartKey)
            {
                HttpContext? httpContext = Unsafe.As<HttpContext>(value.Value);
                if (httpContext is not null)
                {
                    Current = httpContext.RequestServices.CreateLoggerContext();
                }
            }
            else if (value.Key == HttpRequestInDiagnosticsObserver.ActivityStopKey)
            {
                Current?.Dispose();
            }
        }

        public static bool IsEnabled(string operationName)
        {
            return operationName is HttpRequestInDiagnosticsObserver.ActivityName
                or HttpRequestInDiagnosticsObserver.ActivityStartKey
                or HttpRequestInDiagnosticsObserver.ActivityStopKey;
        }
    }
}
