using System.Web.Http;
using Correlate.Http;
using Correlate.WebApiTestNet48.ActionFilters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Correlate.WebApiTestNet48;

public static class FilterConfig
{
    public static void Register(HttpConfiguration config, ServiceProvider serviceProvider)
    {
        config.Filters.Add(new CorrelationIdActionFilter(
            serviceProvider.GetRequiredService<ICorrelationContextAccessor>(),
            serviceProvider.GetRequiredService<IOptions<CorrelateClientOptions>>(),
            serviceProvider.GetRequiredService<IActivityFactory>()));
    }
}
