using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Correlate.DependencyInjection;
using Correlate.WebApiTestNet48.Extensions;
using Correlate.WebApiTestNet48.Middlewares;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Correlate.WebApiTestNet48;

public class WebApiApplication : HttpApplication
{
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(WebApiConfig.Register);
        GlobalConfiguration.Configure(SwaggerConfig.Register);
            
        var services = new ServiceCollection();
        services.AddLogging(x =>
        {
            // we have no console in IIS express so we use Debug instead
            x.AddDebug();
            x.SetMinimumLevel(LogLevel.Trace);
        });

        services.AddCorrelateNet48();
        services.AddCorrelate(opts => opts.IncludeInResponse = true);
        
        ServiceProvider serviceProvider = services.BuildServiceProvider();
        GlobalConfiguration.Configuration.DependencyResolver = new DependencyResolver(serviceProvider);
    }

    protected void Application_BeginRequest()
    {
        IDependencyResolver? resolver = GlobalConfiguration.Configuration.DependencyResolver;
        ICorrelateFeatureNet48 correlateFeatureNet48 = (ICorrelateFeatureNet48)resolver.GetService(typeof(ICorrelateFeatureNet48)) ?? throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered.");
        correlateFeatureNet48.StartCorrelating(Context);
    }
    
    protected void Application_PreSendRequestHeaders()
    {
        IDependencyResolver? resolver = GlobalConfiguration.Configuration.DependencyResolver;
        ICorrelateFeatureNet48 correlateFeatureNet48 = (ICorrelateFeatureNet48)resolver.GetService(typeof(ICorrelateFeatureNet48)) ?? throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered.");
        correlateFeatureNet48.StopCorrelating(Context);
    }
}
