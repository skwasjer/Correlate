using BenchmarkDotNet.Attributes;
using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Correlate.Benchmarks;

#if RELEASE
[MemoryDiagnoser]
[BenchmarkDotNet.Diagnostics.Windows.Configs.InliningDiagnoser(false, new[] { "Correlate.AspNetCore" })]
#endif
[StopOnFirstError]
[MarkdownExporterAttribute.GitHub]
public class AspNetCoreBenchmark
{
    private WebApplication? _app;
    private HttpClient? _httpClient;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        const string hostUrl = "http://localhost:5000";
        _httpClient = new HttpClient { BaseAddress = new Uri(hostUrl) };

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        builder.Logging.ClearProviders();
        builder.WebHost.UseUrls(hostUrl);
        builder.Services.AddCorrelate();

        _app = builder.Build();
        _app.UseCorrelate();
        _app.MapGet("/hello",
            (ICorrelationContextAccessor ctx) =>
            {
                if (ctx.CorrelationContext?.CorrelationId is not null)
                {
                    return;
                }

                throw new InvalidOperationException("No correlation ID.");
            });

        await _app.StartAsync();
    }

    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        _httpClient?.Dispose();
        if (_app is null)
        {
            return;
        }

        await _app.StopAsync();
        await _app.DisposeAsync();
    }

    [Benchmark]
    public async Task ApiCall()
    {
        using HttpResponseMessage response = await _httpClient!.GetAsync("hello");
        response.EnsureSuccessStatusCode();
    }
}
