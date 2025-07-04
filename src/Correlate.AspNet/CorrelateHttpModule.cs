using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Correlate.AspNet.Middlewares;

namespace Correlate.AspNet;

/// <summary>
/// ASP.NET HTTP module for correlating requests and responses with correlation IDs.
/// </summary>
[ExcludeFromCodeCoverage]
// ReSharper disable once UnusedType.Global - Used from Web.config
public class CorrelateHttpModule : IHttpModule
{
    private static bool _initialized;
    private static readonly object Lock = new();

    /// <summary>
    /// Initializes the Correlate HTTP module and sets up the necessary event handlers for request and response correlation.
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Init(HttpApplication context)
    {
        InitializeOnce();
        
#pragma warning disable CA1062
        context.BeginRequest += (sender, _) =>
#pragma warning restore CA1062
        {
            IDependencyResolver? resolver = GlobalConfiguration.Configuration.DependencyResolver;
            ICorrelateFeatureNet48 correlateFeatureNet48 = (ICorrelateFeatureNet48)resolver.GetService(typeof(ICorrelateFeatureNet48))
             ?? throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered.");
            correlateFeatureNet48.StartCorrelating(new HttpContextWrapper(((HttpApplication)sender).Context));
        };

        context.PreSendRequestHeaders += (sender, _) =>
        {
            IDependencyResolver? resolver = GlobalConfiguration.Configuration.DependencyResolver;
            ICorrelateFeatureNet48 correlateFeatureNet48 = (ICorrelateFeatureNet48)resolver.GetService(typeof(ICorrelateFeatureNet48))
             ?? throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered.");
            correlateFeatureNet48.StopCorrelating(new HttpContextWrapper(((HttpApplication)sender).Context));
        };
    }

    private static void InitializeOnce()
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
    }
    
    private static void SetupDependencyResolver()
    {
        switch (GlobalConfiguration.Configuration.DependencyResolver.GetType().Name)
        {
            case "EmptyResolver":
            {
                // This is the default resolver used by Web API if GlobalConfiguration.Configuration.DependencyResolver haven't been set.
                throw new InvalidOperationException("Correlate needs a resolver to handle its dependencies. Please use Microsoft.Extensions.DependencyInjection or similar.");
            }
            
            default:
            {
                IDependencyResolver resolver = GlobalConfiguration.Configuration.DependencyResolver;
                _ = resolver.GetService(typeof(ICorrelationContextFactory)) ?? 
                    throw new InvalidOperationException("ICorrelationContextFactory service is not registered in the current dependency resolver. " +
                        "Please ensure that you have setup your dependency injection correctly.");
                _ = resolver.GetService(typeof(ICorrelateFeatureNet48)) ??
                    throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered in the current dependency resolver. " +
                        "Please ensure that you have setup your dependency injection correctly.");
                break;
            }
        }
    }

    /// <summary>
    /// Disposes of the Correlate HTTP module.
    /// </summary>
    public void Dispose() { }
}
