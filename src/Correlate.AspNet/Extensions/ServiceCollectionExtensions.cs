using System;
using Correlate.AspNet.Middlewares;
using Correlate.AspNet.Options;
using Correlate.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Correlate.AspNet.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorrelateNet48(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelateFeatureNet48, CorrelateFeatureNet48>();
        services.AddCorrelate();

        return services;
    }

    public static IServiceCollection AddCorrelateNet48(this IServiceCollection services, Action<CorrelateOptionsNet48> configureNet48)
    {
        services.AddSingleton<ICorrelateFeatureNet48, CorrelateFeatureNet48>();
        services.AddCorrelate(configureNet48.Invoke);

        return services;
    }
    
    /// <summary>
    /// Adds services required for using correlation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure <see cref="CorrelateOptionsNet48" />.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    private static IServiceCollection AddCorrelate(this IServiceCollection services, Action<CorrelateOptionsNet48> configureOptions)
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

        return services;
    }
}
