using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Correlate.AspNetCore.Fixtures
{
	[Route("")]
	public class TestController : Controller
	{
		private readonly ILogger<TestController> _logger;
		private readonly ICorrelationContextAccessor _correlationContextAccessor;
		private readonly HttpClient _httpClient;

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
		public IActionResult Get()
		{
			_logger.LogInformation("controller action: ok - {Id}", _correlationContextAccessor.CorrelationContext.CorrelationId);

			return Ok("ok");
		}

		/// <summary>
		/// This action calls an external client (mock), and is used to assert that the correlation id is forwarded with that request.
		/// </summary>
		[HttpGet("correlate_client_request")]
		public async Task<IActionResult> CorrelateClientRequest()
		{
			_logger.LogInformation("controller action: ok - {Id}", _correlationContextAccessor.CorrelationContext.CorrelationId);

			HttpResponseMessage response = await _httpClient.GetAsync("correlated_external_call");

			return StatusCode((int)response.StatusCode, response.ReasonPhrase);
		}
	}
}
