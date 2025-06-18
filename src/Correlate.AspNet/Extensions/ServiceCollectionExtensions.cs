using Correlate.AspNet.Middlewares;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.WebApiTestNet48.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorrelateNet48(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelateFeatureNet48, CorrelateFeatureNet48>();
        
        return services;
    }
}
