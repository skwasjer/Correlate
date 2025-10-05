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

    bool IHttpListenerContext.TryGetRequestHeader(string key, out string? value)
    {
        bool result = RequestHeaders.TryGetValue(key, out StringValues values);
        value = values.Count > 0 ? values[0] : null;
        return result;
    }

    bool IHttpListenerContext.TryAddResponseHeader(string key, string? value)
    {
        return ResponseHeaders.TryAdd(key, value);
    }
}
