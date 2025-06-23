using Correlate.AspNet.Middlewares;
using Correlate.AspNet.Options;
using Correlate.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Correlate.AspNet.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceCollection"/> to add Correlate services for .NET Framework 4.8.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for using correlation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddCorrelateNet48(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelateFeatureNet48, CorrelateFeatureNet48>();
        services.AddCorrelate();

        return services;
    }

    /// <summary>
    /// Adds services required for using correlation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="configureNet48">The action used to configure <see cref="CorrelateOptionsNet48" />.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddCorrelateNet48(this IServiceCollection services, Action<CorrelateOptionsNet48> configureNet48)
    {
        services.AddSingleton<ICorrelateFeatureNet48, CorrelateFeatureNet48>();
#pragma warning disable CA1062
        services.AddCorrelate(configureNet48.Invoke);
#pragma warning restore CA1062

        return services;
    }

    /// <summary>
    /// Adds services required for using correlation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure <see cref="CorrelateOptionsNet48" />.</param>
    private static void AddCorrelate(this IServiceCollection services, Action<CorrelateOptionsNet48> configureOptions)
    {
        services
            .Configure(configureOptions);

        services.AddOptions<CorrelationManagerOptions>()
            .Configure((CorrelationManagerOptions cmo, IOptions<CorrelateOptionsNet48> co) =>
            {
                cmo.LoggingScopeKey = co.Value.LoggingScopeKey;
            });

        services
            .AddCorrelate();
    }
}
