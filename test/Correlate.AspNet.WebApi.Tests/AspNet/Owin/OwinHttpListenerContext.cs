using Correlate.Http.Server;
using Microsoft.Owin;

namespace Correlate.AspNet.Owin;

internal sealed class OwinHttpListenerContext(IOwinContext owinContext) : IHttpListenerContext
{
    public IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

    public void OnStartingResponse(Action callback)
    {
        owinContext.Response.OnSendingHeaders(_ => callback(), null);
    }

    public bool TryGetRequestHeader(string key, out string?[]? values)
    {
        return owinContext.Request.Headers.TryGetValue(key, out values);
    }

    public bool TryAddResponseHeader(string key, string?[]? values)
    {
        return owinContext.Response.Headers.TryAdd(key, values);
    }
}

static file class Extensions
{
    internal static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary.ContainsKey(key))
        {
            return false;
        }

        dictionary.Add(key, value);
        return true;
    }
}
