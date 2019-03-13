using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Correlate.DependencyInjection
{
	// ReSharper disable once InconsistentNaming
	public static class IServiceCollectionExtensions
	{
		/// <summary>
		/// Adds services required for using correlation.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
		/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
		public static IServiceCollection AddCorrelate(this IServiceCollection services)
		{
			return services.AddCorrelate(options => {});
		}

		/// <summary>
		/// Adds services required for using correlation.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
		/// <param name="configuration">The <see cref="IConfiguration"/> used to configure <see cref="CorrelateOptions"/>.</param>
		/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
		public static IServiceCollection AddCorrelate(this IServiceCollection services, IConfiguration configuration)
		{
			if (configuration == null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			return services.AddCorrelate(configuration.Bind);
		}

		/// <summary>
		/// Adds services required for using correlation.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
		/// <param name="configureOptions">The action used to configure <see cref="CorrelateOptions"/>.</param>
		/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
		public static IServiceCollection AddCorrelate(this IServiceCollection services, Action<CorrelateOptions> configureOptions)
		{
			services.Configure(configureOptions);

			services.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
			services.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();
			services.TryAddSingleton<ICorrelationIdFactory, GuidCorrelationIdFactory>();

			return services;
		}
	}
}