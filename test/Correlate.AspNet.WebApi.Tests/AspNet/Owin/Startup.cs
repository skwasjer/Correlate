using System.Web.Http;
using Correlate.AspNet.Fixtures;
using Correlate.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MockHttp;
using Owin;

namespace Correlate.AspNet.Owin;

// ReSharper disable once ClassNeverInstantiated.Global - used by TestAppFactory
public class Startup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddCorrelate(opts => opts.IncludeInResponse = true);

        services
            .AddHttpClient<TestController>(client => client.BaseAddress = new Uri("http://0.0.0.0"))
            .ConfigurePrimaryHttpMessageHandler(s => s.GetRequiredService<MockHttpHandler>())
            .CorrelateRequests();
    }

    public void Configuration(IAppBuilder app)
    {
        // For testing only! We can get the root service provider from the app properties.
        // In a real world app, you'd configure the container yourself using your desired method.
        IServiceProvider services = app.Properties["root.serviceProvider"] as IServiceProvider ?? throw new InvalidOperationException("Service container not available.");

        var config = new HttpConfiguration();
        config.DependencyResolver = new DefaultDependencyResolver(services);

        config.MapHttpAttributeRoutes();
        config.Routes.MapHttpRoute(
            "DefaultApi",
            "api/{controller}/{id}",
            new { id = RouteParameter.Optional }
        );

        config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

        config.UseCorrelate();

        app.UseWebApi(config);
    }
}
