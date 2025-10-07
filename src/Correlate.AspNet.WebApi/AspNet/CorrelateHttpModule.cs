using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.Http;
using Correlate.DependencyInjection;
using Correlate.Http.Server;

namespace Correlate.AspNet;

/// <summary>
/// ASP.NET HTTP module for correlating requests and responses with correlation IDs.
/// </summary>
// ReSharper disable once UnusedType.Global - Used from Web.config
public sealed class CorrelateHttpModule() : IHttpModule
{
    private static DefaultHttpListener _listener = null!;
    private static bool _initialized;
    private static object _lock = new();

    private readonly Func<DefaultHttpListener> _httpListenerFactory = static () => GlobalConfiguration.Configuration.DependencyResolver.GetHttpListener();

    internal CorrelateHttpModule(Func<DefaultHttpListener> httpListenerFactory)
        : this()
    {
        _httpListenerFactory = httpListenerFactory;
    }

    void IHttpModule.Init(HttpApplication context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        DefaultHttpListener listener = LazyInitializer.EnsureInitialized(
            ref _listener,
            ref _initialized,
            ref _lock,
            _httpListenerFactory);

        context.BeginRequest += (sender, _) =>
        {
            listener.HandleBeginRequest(new HttpListenerContext(((HttpApplication)sender).Context));
        };

        context.EndRequest += (sender, _) =>
        {
            listener.HandleEndRequest(new HttpListenerContext(((HttpApplication)sender).Context));
        };
    }

    [ExcludeFromCodeCoverage]
    void IHttpModule.Dispose() { }
}
