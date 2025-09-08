using System.Web.Http;
using Correlate.AspNet.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiTestNet48;

public static class DependencyInjectionConfig
{
    public static void Register(HttpConfiguration config)
    {
        var services = new ServiceCollection();
        services.AddCorrelateNet48();
        GlobalConfiguration.Configuration.DependencyResolver = new DefaultDependencyResolver(services.BuildServiceProvider());
    }
}
