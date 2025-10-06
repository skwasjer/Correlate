namespace Correlate.Http.Server;

/// <summary>
/// Defines methods for handling the beginning and end of an HTTP request, enabling correlation ID support for HTTP servers or listeners.
/// </summary>
public interface IHttpListener
{
    /// <summary>
    /// Handles the beginning of an HTTP request, enabling correlation ID support.
    /// </summary>
    /// <param name="context">The <see cref="IHttpListenerContext" /> representing the current HTTP request context.</param>
    void HandleBeginRequest(IHttpListenerContext context);

    /// <summary>
    /// Handles the end of an HTTP request, performing any necessary cleanup or finalization related to correlation ID support.
    /// </summary>
    /// <param name="context">The <see cref="IHttpListenerContext" /> representing the current HTTP request context.</param>
    void HandleEndRequest(IHttpListenerContext context);
}
