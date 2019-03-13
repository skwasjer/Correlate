using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Correlate.AspNetCore.Fixtures
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCorrelate();

			services.AddTransient<TestController>();

			services
				.AddMvcCore()
				.AddControllersAsServices()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseCorrelate();
			app.UseMvc();
		}
	}

	[Route("")]
	public class TestController : Controller
	{
		private readonly ILogger<TestController> _logger;

		public TestController(ILogger<TestController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Get()
		{
			_logger.LogInformation("controller action: ok");
			return Ok("ok");
		}
	}
}