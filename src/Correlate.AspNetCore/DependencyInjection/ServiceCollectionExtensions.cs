using System;
using Correlate.AspNetCore.Middleware;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.DependencyInjection
{
	/// <summary>
	/// Registration extensions for <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds services required for using correlation.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
		/// <param name="configureOptions">The action used to configure <see cref="CorrelateOptions"/>.</param>
		/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
		public static IServiceCollection AddCorrelate(this IServiceCollection services, Action<CorrelateOptions> configureOptions)
		{
			services.Configure(configureOptions);

			return services.AddCorrelate();
		}
	}
}