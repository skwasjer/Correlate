using System.Web;
using Correlate.Http.Server;
using Correlate.Internal;

namespace Correlate.AspNet;

internal sealed class HttpListenerContext(HttpContextBase httpContext) : IHttpListenerContext
{
    public HttpListenerContext(HttpContext httpContext)
        : this(new HttpContextWrapper(httpContext))
    {
    }

    public IDictionary<object, object?> Items { get; } = new GenericDictionaryAdapter(httpContext.Items);

    public void OnStartingResponse(Action callback)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (callback is null)
        {
            return;
        }

        httpContext.Response.AddOnSendingHeaders(_ => callback());
    }

    public bool TryGetRequestHeader(string key, out string?[]? values)
    {
        values = httpContext.Request.Headers.GetValues(key);
        return values is not null;
    }

    public bool TryAddResponseHeader(string key, string?[]? values)
    {
        if (httpContext.Response.Headers.GetValues(key) is not null || values is null)
        {
            return false;
        }

        foreach (string? v in values)
        {
            httpContext.Response.Headers.Add(key, v);
        }

        return true;
    }
}
