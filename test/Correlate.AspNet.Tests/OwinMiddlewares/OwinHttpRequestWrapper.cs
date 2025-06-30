using System.Collections.Specialized;
using System.Web;
using Microsoft.Owin;

namespace Correlate.AspNet.Tests.OwinMiddlewares;

public class OwinHttpRequestWrapper : HttpRequestBase
{
    private readonly NameValueCollection _headers = new NameValueCollection();
    
    public OwinHttpRequestWrapper(IOwinRequest request)
    {
        foreach (KeyValuePair<string, string[]> header in request.Headers)
        {
            foreach (string value in header.Value)
            {
                _headers.Add(header.Key, value);
            }
        }
    }

    public override NameValueCollection Headers => _headers;
}
