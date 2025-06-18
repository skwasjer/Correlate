using System;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Correlate.AspNet.Middlewares;
using Correlate.DependencyInjection;
using Correlate.WebApiTestNet48.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Correlate.AspNet;

public class CorrelateHttpModule : IHttpModule
{
    private static bool _initialized;
    private static readonly object Lock = new();

    public void Init(HttpApplication context)
    {
        if (!_initialized)
        {
            lock (Lock)
            {
                if (!_initialized)
                {
                    SetupDependencyResolver();
                    _initialized = true;
                }
            }
        }
        
        context.BeginRequest += (sender, _) =>
        {
            IDependencyResolver? resolver = GlobalConfiguration.Configuration.DependencyResolver;
            ICorrelateFeatureNet48 correlateFeatureNet48 = (ICorrelateFeatureNet48)resolver.GetService(typeof(ICorrelateFeatureNet48))
             ?? throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered.");
            correlateFeatureNet48.StartCorrelating(((HttpApplication)sender).Context);
        };

        context.PreSendRequestHeaders += (sender, _) =>
        {
            IDependencyResolver? resolver = GlobalConfiguration.Configuration.DependencyResolver;
            ICorrelateFeatureNet48 correlateFeatureNet48 = (ICorrelateFeatureNet48)resolver.GetService(typeof(ICorrelateFeatureNet48))
             ?? throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered.");
            correlateFeatureNet48.StopCorrelating(((HttpApplication)sender).Context);
        };
    }

    private static void SetupDependencyResolver()
    {
        switch (GlobalConfiguration.Configuration.DependencyResolver.GetType().Name)
        {
            // This  is the default resolver used by Web API if GlobalConfiguration.Configuration.DependencyResolver haven't been set.
            case "EmptyResolver":
            {
                var services = new ServiceCollection();
                services.AddLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Trace);
                });
            
                services.AddCorrelateNet48();
                services.AddCorrelate(opts => opts.IncludeInResponse = true);
            
                ServiceProvider serviceProvider = services.BuildServiceProvider();
                GlobalConfiguration.Configuration.DependencyResolver = new DependencyResolver(serviceProvider);
                break;
            }
            
            default:
            {
                IDependencyResolver resolver = GlobalConfiguration.Configuration.DependencyResolver;
                _ = resolver.GetService(typeof(ICorrelationIdFactory)) ?? 
                    throw new InvalidOperationException("ICorrelationIdFactory service is not registered in the current dependency resolver. " +
                        "Please ensure that you have called services.AddCorrelate() in your dependency injection setup.");
                _ = resolver.GetService(typeof(ICorrelateFeatureNet48)) ??
                    throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered in the current dependency resolver. " +
                        "Please ensure that you have called services.AddCorrelateNet48() in your dependency injection setup.");
                break;
            }
        }
    }

    public void Dispose() { }
}
