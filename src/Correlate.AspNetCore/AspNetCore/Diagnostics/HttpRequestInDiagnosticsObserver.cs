using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Correlate.Http.Server;
using Microsoft.AspNetCore.Http;

namespace Correlate.AspNetCore.Diagnostics;

internal sealed class HttpRequestInDiagnosticsObserver : IObserver<KeyValuePair<string, object?>>
{
    internal const string ActivityName = "Microsoft.AspNetCore.Hosting.HttpRequestIn";
    internal const string ActivityStartKey = ActivityName + ".Start";
    internal const string ActivityStopKey = ActivityName + ".Stop";

    private readonly IHttpListener _listener;

    public HttpRequestInDiagnosticsObserver(IHttpListener listener)
    {
        _listener = listener;
    }

    [ExcludeFromCodeCoverage]
    public void OnCompleted()
    {
        // N/A
    }

    [ExcludeFromCodeCoverage]
    public void OnError(Exception error)
    {
        // N/A
    }

    public void OnNext(KeyValuePair<string, object?> value)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (value.Key == ActivityStartKey)
        {
            HttpContext? httpContext = Unsafe.As<HttpContext>(value.Value);
            HandleHttpRequestInStart(httpContext, _listener);
        }
        else if (value.Key == ActivityStopKey)
        {
            HttpContext? httpContext = Unsafe.As<HttpContext>(value.Value);
            HandleHttpRequestInStop(httpContext);
        }
    }

    internal static bool IsEnabled(string operationName)
    {
        return operationName == ActivityName
            // ReSharper disable once MergeIntoLogicalPattern
         || operationName == ActivityStartKey
         || operationName == ActivityStopKey;
    }

    // NoInlining because we want to preserve the call stack for these core methods.

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleHttpRequestInStart(HttpContext? httpContext, IHttpListener listener)
    {
        if (httpContext is null)
        {
            return;
        }

        // Add our correlate request handler as a feature.
        httpContext.Features.Set(listener);
        listener.HandleBeginRequest(new HttpListenerContext(httpContext));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void HandleHttpRequestInStop(HttpContext? httpContext)
    {
        httpContext?.Features.Get<IHttpListener>()?.HandleEndRequest(new HttpListenerContext(httpContext));
    }
}
