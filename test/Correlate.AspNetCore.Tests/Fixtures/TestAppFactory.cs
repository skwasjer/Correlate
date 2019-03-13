using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Serilog;

namespace Correlate.AspNetCore.Fixtures
{
	public class TestAppFactory<TStartup> : WebApplicationFactory<TStartup>
		where TStartup : class
	{
		protected override IWebHostBuilder CreateWebHostBuilder()
		{
			return WebHost.CreateDefaultBuilder()
				.UseStartup<TStartup>()
				.UseSerilog((context, configuration) =>
				{
					configuration
						.MinimumLevel.Information()
						.WriteTo.TestCorrelator();
				});
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			builder.UseContentRoot(".");
			base.ConfigureWebHost(builder);
		}
	}
}