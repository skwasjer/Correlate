using System;
using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;

namespace Correlate.AspNetCore.Fixtures
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCorrelate();

			services
				.AddHttpClient<TestController>(client => client.BaseAddress = new Uri("http://0.0.0.0"))
				.ConfigurePrimaryHttpMessageHandler(s => s.GetRequiredService<MockHttpMessageHandler>())
				.CorrelateRequests();

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
}