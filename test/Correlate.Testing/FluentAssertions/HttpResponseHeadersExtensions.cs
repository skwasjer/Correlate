using System.Net.Http.Headers;

namespace Correlate.Testing.FluentAssertions;

public static class HttpResponseHeadersExtensions
{
    public static ResponseHeadersAssertions Should(this HttpResponseHeaders actualValue)
    {
        return new ResponseHeadersAssertions(actualValue);
    }
}
