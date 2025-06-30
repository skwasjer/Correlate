using System.Web.Http;
using Correlate.AspNet.Extensions;
using Correlate.AspNet.Tests.OwinMiddlewares;
using Correlate.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MockHttp;
using Owin;

namespace Correlate.AspNet.Tests.Fixtures;

public class Startup
{
    public ServiceProvider? ServiceProvider { get; private set;}

    public void Configuration(IAppBuilder app, Action<IServiceCollection> servicesAction)
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        servicesAction(services);

        ServiceProvider = services.BuildServiceProvider();

        var config = new HttpConfiguration();
        config.DependencyResolver = new DefaultDependencyResolver(ServiceProvider);

        config.MapHttpAttributeRoutes();
        config.Routes.MapHttpRoute(
            name: "DefaultApi",
            routeTemplate: "api/{controller}/{id}",
            defaults: new { id = RouteParameter.Optional }
        );

        config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

        GlobalConfiguration.Configuration.DependencyResolver = new DefaultDependencyResolver(ServiceProvider);

        config.MessageHandlers.Add(new CorrelateMessageHandler());
        
        app.UseWebApi(config);
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddCorrelateNet48(opts => opts.IncludeInResponse = true);

        services
            .AddHttpClient<TestController>(client => client.BaseAddress = new Uri("http://0.0.0.0"))
            .ConfigurePrimaryHttpMessageHandler(s => s.GetRequiredService<MockHttpHandler>())
            .CorrelateRequests();
    }
}
