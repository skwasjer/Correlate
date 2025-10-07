using System.Net.Http;
using Correlate.Http.Server;
using Microsoft.Owin;

namespace Correlate.AspNet.Owin;

public class CorrelateMessageHandler : DelegatingHandler
{
    private readonly IHttpListener _listener;

    internal CorrelateMessageHandler(IHttpListener listener)
    {
        _listener = listener ?? throw new ArgumentNullException(nameof(listener));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        IOwinContext owinContext = request.GetOwinContext();
        var ctx = new OwinHttpListenerContext(owinContext);

        _listener.HandleBeginRequest(ctx);

        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            _listener.HandleBeginRequest(ctx);
        }
    }
}
