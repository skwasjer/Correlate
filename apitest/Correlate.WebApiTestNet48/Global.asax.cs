using System.Web;
using System.Web.Http;
using Correlate.AspNet;
using Correlate.AspNet.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Correlate.WebApiTestNet48;

public class WebApiApplication : HttpApplication
{
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(WebApiConfig.Register);
        GlobalConfiguration.Configure(SwaggerConfig.Register);

        // this can be removed and the DI will be setup in Correlate.Aspnet HTTP module
        SetupDependencyResolver();
    }

    private static void SetupDependencyResolver()
    {
        var services = new ServiceCollection();
        services.AddLogging(x =>
        {
            x.AddConsole();
            x.SetMinimumLevel(LogLevel.Trace);
        });

        services.AddCorrelateNet48(opts => opts.IncludeInResponse = true);

        ServiceProvider serviceProvider = services.BuildServiceProvider();
        GlobalConfiguration.Configuration.DependencyResolver = new DependencyResolver(serviceProvider);
    }
}
