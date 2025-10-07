using Correlate.AspNet.Owin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Testing;

namespace Correlate.AspNet.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global - used in IntegrationTests.cs
public class TestAppFactory<TStartup>
    where TStartup : class
{
    private readonly DynamicLevelFilter _logLevelSwitch = new();
    private LogLevel? _lastLevel = LogLevel.Information;

    public TestServerWrapper CreateServer(Action<IServiceCollection> configureServices)
    {
        ServiceProvider? serviceProvider = null;
        var server = TestServer.Create(app =>
        {
            TStartup startup = Activator.CreateInstance<TStartup>();

            IServiceCollection services = new ServiceCollection()
                .AddLogging(builder => builder
                    .ClearProviders()
                    .SetMinimumLevel(LogLevel.Trace)
                    .AddFakeLogging()
                    .AddDebug()
                    .Services.Configure<LoggerFilterOptions>(opts => opts.Rules.Add(_logLevelSwitch))
                );
            configureServices(services);
            typeof(TStartup).GetMethod(nameof(Startup.ConfigureServices))?.Invoke(startup, [services]);

            app.Properties["root.serviceProvider"] = serviceProvider = services.BuildServiceProvider();
            typeof(TStartup).GetMethod(nameof(Startup.Configuration))?.Invoke(startup, [app]);
        });
        return new TestServerWrapper(server, serviceProvider ?? throw new InvalidOperationException("Server not initialized."));
    }

    public bool LoggingEnabled
    {
        get => (int?)_logLevelSwitch.LogLevel <= (int)LogLevel.Critical;
        set
        {
            if (value)
            {
                _logLevelSwitch.LogLevel = _lastLevel;
            }
            else
            {
                if ((int?)_lastLevel <= (int)LogLevel.Critical)
                {
                    _lastLevel = _logLevelSwitch.LogLevel;
                }

                _logLevelSwitch.LogLevel = LogLevel.None;
            }
        }
    }

    private class DynamicLevelFilter : LoggerFilterRule
    {
        public static LogLevel? _level;

        public DynamicLevelFilter() : base(
            null,
            null,
            null,
            (s, s1, arg3) =>
            {
                return arg3 >= _level;
            })
        {
        }

        public new LogLevel? LogLevel
        {
            get => _level;
            set => _level = value;
        }
    }
}
