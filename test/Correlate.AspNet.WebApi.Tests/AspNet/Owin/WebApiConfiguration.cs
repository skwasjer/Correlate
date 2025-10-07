using System.Web.Http;
using Correlate.DependencyInjection;
using Correlate.Http.Server;

namespace Correlate.AspNet.Owin;

public static class WebApiConfiguration
{
    public static HttpConfiguration UseCorrelate(this HttpConfiguration config)
    {
        if (config is null)
        {
            throw new ArgumentNullException(nameof(config));
        }

        DefaultHttpListener listener = config.DependencyResolver.GetHttpListener();
        config.MessageHandlers.Add(new CorrelateMessageHandler(listener));
        return config;
    }
}
