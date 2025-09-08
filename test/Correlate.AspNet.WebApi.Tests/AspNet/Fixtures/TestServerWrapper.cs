using System.Net.Http;
using Microsoft.Owin.Testing;

namespace Correlate.AspNet.Fixtures;

public sealed class TestServerWrapper(TestServer server, IServiceProvider services) : IDisposable
{
    public HttpClient CreateClient() => server.HttpClient;
    public IServiceProvider Services { get; } = services;

    public void Dispose()
    {
        server.Dispose();
        (Services as IDisposable)?.Dispose();
    }
}
