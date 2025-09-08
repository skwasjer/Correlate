using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dependencies;
using Correlate.AspNet.Middlewares;
using Microsoft.Owin;

namespace Correlate.AspNet.Tests.OwinMiddlewares;

public class CorrelateMessageHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        IDependencyResolver? resolver = GlobalConfiguration.Configuration.DependencyResolver;
        ICorrelateFeatureNet48 correlateFeatureNet48 = (ICorrelateFeatureNet48)resolver.GetService(typeof(ICorrelateFeatureNet48))
         ?? throw new InvalidOperationException("CorrelateFeatureNet48 service is not registered.");

        IOwinContext owinContext = request.GetOwinContext();
        var httpContext = new OwinHttpContextWrapper(owinContext);

        correlateFeatureNet48.StartCorrelating(httpContext);

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            correlateFeatureNet48.StopCorrelating(httpContext);

            foreach (string key in httpContext.Response.Headers.AllKeys)
            {
                owinContext.Response.Headers.Append(key, httpContext.Response.Headers[key]);
            }
        }
        
        return response;
    }
}
