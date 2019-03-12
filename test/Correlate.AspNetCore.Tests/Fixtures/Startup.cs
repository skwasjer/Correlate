using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.AspNetCore.Fixtures
{
	public class Startup
	{
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddCorrelate();

			services
				.AddMvcCore()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseCorrelate();
			app.UseMvc();
		}
	}
}