using System.Collections.Specialized;
using System.Web;
using Microsoft.Owin;

namespace Correlate.AspNet.Tests.OwinMiddlewares;

public class OwinHttpResponseWrapper : HttpResponseBase
{
    private readonly IOwinResponse _response;
    private readonly NameValueCollection _headers = new();

    public OwinHttpResponseWrapper(IOwinResponse response)
    {
        _response = response;

        foreach (KeyValuePair<string, string[]> header in response.Headers)
        {
            foreach (string value in header.Value)
            {
                _headers.Add(header.Key, value);
            }
        }
    }

    public override NameValueCollection Headers => _headers;
}
