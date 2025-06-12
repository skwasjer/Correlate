using System.Web.Http;
using Correlate.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.WebApiTestNet48;

public class WebApiApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(WebApiConfig.Register);
        GlobalConfiguration.Configure(SwaggerConfig.Register);
            
        var services = new ServiceCollection();
        services.AddCorrelate();
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        GlobalConfiguration.Configuration.DependencyResolver = new DependencyResolver(serviceProvider);
        
        GlobalConfiguration.Configure(x => FilterConfig.Register(x, serviceProvider));
    }
}
