using Correlate;
using Microsoft.AspNetCore.Mvc;

namespace Service.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly ICorrelationContextAccessor _correlationContextAccessor;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, ICorrelationContextAccessor correlationContextAccessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get()
    {
        _logger.LogInformation("Logging with ambient Correlation ID.");
#pragma warning disable CA2254 // Justification: intentional for the purpose of this test.
        _logger.LogInformation($"Logging ID from context accessor: '{_correlationContextAccessor.CorrelationContext?.CorrelationId}'.");
#pragma warning restore CA2254

        var rng = new Random();
        return Enumerable.Range(1, 5)
            .Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
    }
}
