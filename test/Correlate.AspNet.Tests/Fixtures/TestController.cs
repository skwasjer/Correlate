using System.Net.Http;
using System.Web.Http;
using Microsoft.Extensions.Logging;

namespace Correlate.AspNet.Tests.Fixtures;

[Route("")]
public sealed class TestController : ApiController
{
    private readonly ICorrelationContextAccessor _correlationContextAccessor;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger, ICorrelationContextAccessor correlationContextAccessor, HttpClient httpClient)
    {
        _logger = logger;
        _correlationContextAccessor = correlationContextAccessor;
        _httpClient = httpClient;
    }

    /// <summary>
    /// This action simply returns OK.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IHttpActionResult Get()
    {
        _logger.LogInformation($"controller action: ok - {_correlationContextAccessor.CorrelationContext?.CorrelationId}");
        _logger.LogInformation("Log property {Property}", _correlationContextAccessor.CorrelationContext?.CorrelationId);

        return Ok("ok");
    }

    /// <summary>
    /// This action calls an external client (mock), and is used to assert that the correlation id is forwarded with that request.
    /// </summary>
    [HttpGet]
    [Route("correlate_client_request")]
    public async Task<IHttpActionResult> CorrelateClientRequest()
    {
        _logger.LogInformation($"controller action: ok - {_correlationContextAccessor.CorrelationContext?.CorrelationId}");
        _logger.LogInformation("Log property {Property}", _correlationContextAccessor.CorrelationContext?.CorrelationId);

        HttpResponseMessage response = await _httpClient.GetAsync("correlated_external_call");

        return StatusCode(response.StatusCode);
    }
}
