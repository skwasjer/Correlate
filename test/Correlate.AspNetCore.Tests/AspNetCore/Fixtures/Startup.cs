using Correlate.DependencyInjection;
using MockHttp;
using Serilog.Sinks.TestCorrelator;

namespace Correlate.AspNetCore.Fixtures;

public class Startup
{
    public static ITestCorrelatorContext LastRequestContext { get; private set; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCorrelate();

        services
            .AddHttpClient<TestController>(client => client.BaseAddress = new Uri("http://0.0.0.0"))
            .ConfigurePrimaryHttpMessageHandler(s => s.GetRequiredService<MockHttpHandler>())
            .CorrelateRequests();

        services
            .AddControllers()
            .AddControllersAsServices();
    }

    public void Configure(IApplicationBuilder app)
    {
        // Create context to track log events.
        app.UseMiddleware<TestContextMiddleware>();

        app.UseCorrelate();

        app.UseRouting();
        app.UseEndpoints(builder => builder.MapControllers());
    }

    private class TestContextMiddleware
    {
        private readonly RequestDelegate _next;

        public TestContextMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task Invoke(HttpContext httpContext)
        {
            using (LastRequestContext = TestCorrelator.CreateContext())
            {
                await _next.Invoke(httpContext);
            }
        }
    }
}
