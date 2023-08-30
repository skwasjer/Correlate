using Client;
using Correlate.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost host = CreateHostBuilder(args).Build();
using (host)
{
    await host.StartAsync();
    await host.WaitForShutdownAsync();
}

return;

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .UseConsoleLifetime()
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<WeatherChecker>();

            services
                .AddHttpClient<WeatherApiClient>(client => client.BaseAddress = context.Configuration.GetValue<Uri>("WeatherClient:Uri"))
                .CorrelateRequests();
        });
}
