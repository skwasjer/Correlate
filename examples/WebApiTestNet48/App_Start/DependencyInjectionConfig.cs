using System.Web.Http;
using Correlate.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiTestNet48;

public static class DependencyInjectionConfig
{
    public static void Register(HttpConfiguration config)
    {
        ServiceProvider services = new ServiceCollection()
            // Register dependencies.
            .AddCorrelate()
            .BuildServiceProvider();

        // Create and set the adapter.
        config.DependencyResolver = new DefaultDependencyResolver(services);
    }
}
