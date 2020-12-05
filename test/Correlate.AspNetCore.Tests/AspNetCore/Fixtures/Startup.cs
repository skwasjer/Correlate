using System;
using System.Threading.Tasks;
using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MockHttp;
using Serilog.Sinks.TestCorrelator;

namespace Correlate.AspNetCore.Fixtures
{
	public class Startup
	{
		public static ITestCorrelatorContext LastRequestContext { get; private set; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCorrelate();

			services
				.AddHttpClient<TestController>(client => client.BaseAddress = new Uri("http://0.0.0.0"))
				.ConfigurePrimaryHttpMessageHandler(s => s.GetRequiredService<MockHttpHandler>())
				.CorrelateRequests();

#if NETCOREAPP3_1 || NET5_0
			services
				.AddControllers()
				.AddControllersAsServices();
#else
			services
				.AddMvcCore()
				.AddControllersAsServices()
#if NETCOREAPP2_1
				.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
#else
				.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_2);
#endif
#endif
		}

		public void Configure(IApplicationBuilder app)
		{
			// Create context to track log events.
			app.UseMiddleware<TestContextMiddleware>();

			app.UseCorrelate();

#if NETCOREAPP3_1 || NET5_0
			app.UseRouting();
			app.UseEndpoints(builder => builder.MapControllers());
#else
			app.UseMvc();
#endif
		}

		private class TestContextMiddleware
		{
			private readonly RequestDelegate _next;

			public TestContextMiddleware(RequestDelegate next)
			{
				_next = next ?? throw new ArgumentNullException(nameof(next));
			}

			public async Task Invoke(HttpContext httpContext)
			{
				using (LastRequestContext = TestCorrelator.CreateContext())
				{
					await _next.Invoke(httpContext);
				}
			}
		}
	}
}
