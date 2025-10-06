using Correlate.Http.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Correlate.AspNetCore;

internal sealed class HttpListenerContext(HttpContext httpContext) : IHttpListenerContext
{
    public IDictionary<object, object?> Items { get; } = httpContext.Items;

    public void OnStartingResponse(Action callback)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (callback is null)
        {
            return;
        }

        httpContext.Response.OnStarting(() =>
        {
            callback();
            return Task.CompletedTask;
        });
    }

    public bool TryGetRequestHeader(string key, out string?[]? values)
    {
        if (!httpContext.Request.Headers.TryGetValue(key, out StringValues headerValues))
        {
            values = null;
            return false;
        }

        values = headerValues;
        return true;
    }

    public bool TryAddResponseHeader(string key, string?[]? values)
    {
        return httpContext.Response.Headers.TryAdd(key, values);
    }
}
