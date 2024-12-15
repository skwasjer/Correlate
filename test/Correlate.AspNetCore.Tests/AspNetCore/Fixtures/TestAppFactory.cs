using Microsoft.AspNetCore.Mvc.Testing;

namespace Correlate.AspNetCore.Fixtures;

public class TestAppFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly DynamicLevelFilter _logLevelSwitch = new();
    private LogLevel? _lastLevel = LogLevel.Information;

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

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseEnvironment(Environments.Development)
            .ConfigureLogging(builder => builder
                .ClearProviders()
                .SetMinimumLevel(LogLevel.Trace)
                .AddFakeLogging()
                .AddDebug()
                .Services.Configure<LoggerFilterOptions>(opts => opts.Rules.Add(_logLevelSwitch))
            )
            .ConfigureWebHost(webHostBuilder => webHostBuilder
                .UseStartup<TStartup>()
            );
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(".");
        base.ConfigureWebHost(builder);
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
