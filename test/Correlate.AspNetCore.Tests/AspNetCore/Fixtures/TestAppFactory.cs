using Microsoft.AspNetCore.Mvc.Testing;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Correlate.AspNetCore.Fixtures;

public class TestAppFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private readonly LoggingLevelSwitch _logLevelSwitch = new();
    private LogEventLevel _lastLevel = LogEventLevel.Information;

    public bool LoggingEnabled
    {
        get => (int)_logLevelSwitch.MinimumLevel <= (int)LogEventLevel.Fatal;
        set
        {
            if (value)
            {
                _logLevelSwitch.MinimumLevel = _lastLevel;
            }
            else
            {
                if ((int)_lastLevel <= (int)LogEventLevel.Fatal)
                {
                    _lastLevel = _logLevelSwitch.MinimumLevel;
                }

                _logLevelSwitch.MinimumLevel = (LogEventLevel)10 + (int)LogEventLevel.Fatal;
            }
        }
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .UseEnvironment(Environments.Development)
            .UseSerilog((_, configuration) =>
                {
                    configuration
                        .MinimumLevel.ControlledBy(_logLevelSwitch)
                        .WriteTo.TestCorrelator();
                },
                true)
            .ConfigureWebHost(webHostBuilder => webHostBuilder
                .UseStartup<TStartup>()
            );
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(".");
        base.ConfigureWebHost(builder);
    }
}
