using Correlate;

namespace Client;

public class WeatherApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public WeatherApiClient(HttpClient httpClient, ICorrelationContextAccessor correlationContextAccessor)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
    }

    public async Task<IEnumerable<WeatherForecast>> GetForecast(CancellationToken cancellationToken)
    {
        // Yes I know this looks stupid :) its an example, don't do this!
        Console.Write("Sending request with correlation ID: ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(_correlationContextAccessor.CorrelationContext?.CorrelationId);
        Console.ResetColor();

        HttpResponseMessage response = await _httpClient.GetAsync("weatherforecast/", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsAsync<IEnumerable<WeatherForecast>>(cancellationToken);
        }

        return Array.Empty<WeatherForecast>();
    }
}
