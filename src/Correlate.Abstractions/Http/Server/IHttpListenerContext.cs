namespace Correlate.Http.Server;

/// <summary>
/// A lightweight representation of an implementation specific 'HttpContext' that is used to add correlation ID support to HTTP servers/listeners.
/// </summary>
public interface IHttpListenerContext
{
    /// <summary>
    /// Gets a dictionary that can be used to store and share data within the context of an HTTP request.
    /// </summary>
    IDictionary<object, object?> Items { get; }

    /// <summary>
    /// Registers a callback to be invoked just before the HTTP response is sent.
    /// </summary>
    /// <param name="callback">The callback to invoke.</param>
    void OnStartingResponse(Action callback);

    /// <summary>
    /// Attempts to retrieve the value of a specified request header.
    /// </summary>
    /// <param name="key">The name of the header to retrieve.</param>
    /// <param name="value">When this method returns, contains the value of the specified header if it exists; otherwise, <c>null</c>.</param>
    /// <returns><c>true</c> if the specified header exists and its value was successfully retrieved; otherwise, <c>false</c>.</returns>
    bool TryGetRequestHeader(string key, out string? value);

    /// <summary>
    /// Attempts to add a header to the HTTP response.
    /// </summary>
    /// <param name="key">The name of the header to add.</param>
    /// <param name="value">The value of the header to add.</param>
    /// <returns><c>true</c> if the header was successfully added; otherwise, <c>false</c>.</returns>
    /// <remarks>If the header already exists, this method does not overwrite the existing value.</remarks>
    bool TryAddResponseHeader(string key, string? value);
}
