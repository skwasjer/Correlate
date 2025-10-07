using Correlate.Http.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
#if NET48_OR_GREATER
using Correlate.AspNet;
#else
using Correlate.AspNetCore;
#endif

namespace Correlate.DependencyInjection;

/// <summary>
/// Registration extensions for <see cref="IServiceCollection" />.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds services required for using correlation.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection" /> to add the services to.</param>
    /// <param name="configureOptions">The action used to configure <see cref="CorrelateOptions" />.</param>
    /// <returns>The <see cref="IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddCorrelate(this IServiceCollection services, Action<CorrelateOptions> configureOptions)
    {
        services
            .Configure(configureOptions);

        services
            .AddOptions<HttpListenerOptions>()
            .Configure<IOptions<CorrelateOptions>>((opts, co) =>
            {
                opts.IncludeInResponse = co.Value.IncludeInResponse;
                opts.RequestHeaders = co.Value.RequestHeaders;
            });

        services.AddOptions<CorrelationManagerOptions>()
            .Configure((CorrelationManagerOptions cmo, IOptions<CorrelateOptions> co) =>
            {
                cmo.LoggingScopeKey = co.Value.LoggingScopeKey;
            });

        services
            .AddCorrelate();

        return services;
    }
}
