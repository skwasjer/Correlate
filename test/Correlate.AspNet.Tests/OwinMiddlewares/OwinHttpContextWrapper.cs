using System.Collections;
using System.Web;
using Microsoft.Owin;

namespace Correlate.AspNet.Tests.OwinMiddlewares;

public class OwinHttpContextWrapper(IOwinContext owinContext) : HttpContextBase
{
    public override HttpRequestBase Request { get; } = new OwinHttpRequestWrapper(owinContext.Request);
    public override HttpResponseBase Response { get; } = new OwinHttpResponseWrapper(owinContext.Response);

    public override IDictionary Items { get; } = new Hashtable();
}
