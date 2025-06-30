using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Testing;

namespace Correlate.AspNet.Tests.Fixtures;

// ReSharper disable once ClassNeverInstantiated.Global - used in IntegrationTests.cs
public class TestAppFactory<TStartup> where TStartup : class
{
    private readonly DynamicLevelFilter _logLevelSwitch = new();
    private LogLevel? _lastLevel = LogLevel.Information;

    public IServiceProvider? ServiceProvider { get; private set; }

    public TestServer CreateServer(Action<IServiceCollection> services)
    {
        return TestServer.Create(app =>
        {
            TStartup startup = Activator.CreateInstance<TStartup>();
            
            // chain another action to services
            var wrappedServices = new Action<IServiceCollection>(x =>
            {
                x.AddLogging(loggingBuilder =>
                {
                    loggingBuilder.AddFakeLogging()
                        .SetMinimumLevel(LogLevel.Trace)
                        .AddFakeLogging()
                        .AddDebug()
                        .Services.Configure<LoggerFilterOptions>(opts => opts.Rules.Add(_logLevelSwitch));
                });
                services(x);
            });
            
            typeof(TStartup).GetMethod(nameof(Startup.Configuration))?.Invoke(startup, [app, wrappedServices]);
            ServiceProvider = ((IServiceProvider)typeof(TStartup).GetProperty(nameof(Startup.ServiceProvider))?.GetValue(startup)!);
        });
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
