using Correlate;
using Microsoft.Extensions.Hosting;

namespace Client;

internal class WeatherChecker : IHostedService
{
    private readonly WeatherApiClient _apiClient;
    private readonly IAsyncCorrelationManager _asyncCorrelationManager;

    public WeatherChecker
    (
        WeatherApiClient apiClient,
        IAsyncCorrelationManager asyncCorrelationManager)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
        _asyncCorrelationManager = asyncCorrelationManager ?? throw new ArgumentNullException(nameof(asyncCorrelationManager));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Console.WriteLine("Type a correlation ID to associate with the request or <ENTER> to generate one:");
            string? correlationId = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(correlationId))
            {
                correlationId = null; // Let Correlate generate one.
            }

            IEnumerable<WeatherForecast> forecasts = await _asyncCorrelationManager.CorrelateAsync(
                correlationId,
                () => _apiClient.GetForecast(cancellationToken)
            );

            Console.WriteLine("The temperature is {0:D}C.", forecasts.FirstOrDefault()?.TemperatureC);
            Console.WriteLine();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
