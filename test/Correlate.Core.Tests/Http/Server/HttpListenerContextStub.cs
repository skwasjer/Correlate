using Microsoft.Extensions.Primitives;

namespace Correlate.Http.Server;

internal sealed class HttpListenerContextStub : IHttpListenerContext
{
    private Action? _callback;
    public Dictionary<string, StringValues> RequestHeaders { get; } = new();

    public Dictionary<string, StringValues> ResponseHeaders { get; } = new();

    public IDictionary<object, object?> Items { get; } = new Dictionary<object, object?>();

    public void FireOnStartingResponse()
    {
        _callback?.Invoke();
    }

    void IHttpListenerContext.OnStartingResponse(Action callback)
    {
        _callback = callback;
    }

    bool IHttpListenerContext.TryGetRequestHeader(string key, out string?[]? values)
    {
        bool result = RequestHeaders.TryGetValue(key, out StringValues sv);
        values = sv;
        return result;
    }

    bool IHttpListenerContext.TryAddResponseHeader(string key, string?[]? values)
    {
        return ResponseHeaders.TryAdd(key, values);
    }
}
