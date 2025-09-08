An ASP.NET (classic/.net framework) implementation to correlate activities between decoupled components (eg. microservices) via a HTTP header.

## Usage

This package provides a managed IIS HTTP module to automatically handle the correlation of HTTP requests and responses. To activate the module, add it to your `web.config` file.

```xml
  <system.webServer>
    <validation validateIntegratedModeConfiguration="false" />
    <modules>
      <add name="CorrelateHttpModule" type="Correlate.AspNet.CorrelateHttpModule, Correlate.AspNet.WebApi" />
    </modules>
  </system.webServer>
  <system.web>
    <httpModules>
      <add name="CorrelateHttpModule" type="Correlate.AspNet.CorrelateHttpModule, Correlate.AspNet.WebApi" />
    </httpModules>
  </system.web>
```

If you already have a `<modules>/<httpModules>` section in your `web.config`, you can simply just add the line with `CorrelateHttpModule`.

## Dependencies

The HTTP module requires certain dependencies to be registered and resolvable from the `GlobalConfiguration.Configuration.DependencyResolver` dependency container.

For convenience, this package comes with an adapter `DefaultDependencyResolver` that is based on `IServiceProvider` (Microsoft's current abstraction). With this adapter, you can use the dependency injection registration extensions provided by the [Correlate.DependencyInjection](https://www.nuget.org/packages/Correlate.DependencyInjection/) package to configure the `DependencyResolver` on `HttpConfiguration`.

> Of course, dependency registration can be done using any DI container of your choice, as long as it supplies all the necessary services.

### Example

#### Global.asax.cs
```csharp
using System.Web.Http;

namespace MyWebApp;

public class WebApiApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        GlobalConfiguration.Configure(WebApiConfig.Register);
        GlobalConfiguration.Configure(DependencyInjectionConfig.Register);
    }
}
```

#### ~/App_Start/DependencyInjectionConfig.cs
```csharp
using System.Web.Http;
using Correlate.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace MyWebApp;

public static class DependencyInjectionConfig
{
    public static void Register(HttpConfiguration config)
    {
        ServiceProvider services = new ServiceCollection()
            // Register dependencies.
            .AddCorrelate()
            .BuildServiceProvider();

        // Create and set the adapter.
        config.DependencyResolver = new DefaultDependencyResolver(services);
    }
}
```
> Note: do not dispose the created `ServiceProvider` when leaving this method. It will be disposed automatically by the adapter on app recycle/shutdown.

### Useful links

- [GitHub / docs](https://github.com/skwasjer/Correlate)
- [Changelog](https://github.com/skwasjer/Correlate/releases)
- [Examples](https://github.com/skwasjer/Correlate/tree/main/examples)
