# Correlate

Correlate provides flexible .NET Core support for correlation ID in ASP.NET Core and HttpClient.

## Installation

Install Correlate via the Nuget package manager or `dotnet` cli.

```powershell
dotnet add package Correlate
```

For ASP.NET Core integration:

```powershell
dotnet add package Correlate.AspNetCore
```

---

[![Build status](https://ci.appveyor.com/api/projects/status/rwfdg9d4i3g0qyga/branch/main?svg=true)](https://ci.appveyor.com/project/skwasjer/correlate)
[![Tests](https://img.shields.io/appveyor/tests/skwasjer/Correlate/main.svg)](https://ci.appveyor.com/project/skwasjer/correlate/build/tests)
[![codecov](https://codecov.io/gh/skwasjer/Correlate/branch/main/graph/badge.svg)](https://codecov.io/gh/skwasjer/Correlate)

| | | |
|---|---|---|
| `Correlate` | [![NuGet](https://img.shields.io/nuget/v/Correlate.svg)](https://www.nuget.org/packages/Correlate/) [![NuGet](https://img.shields.io/nuget/dt/Correlate.svg)](https://www.nuget.org/packages/Correlate/) | Core library, including a `DelegatingHandler` for `HttpClient`. |
| `Correlate.Abstractions` | [![NuGet](https://img.shields.io/nuget/v/Correlate.Abstractions.svg)](https://www.nuget.org/packages/Correlate.Abstractions/) [![NuGet](https://img.shields.io/nuget/dt/Correlate.Abstractions.svg)](https://www.nuget.org/packages/Correlate.Abstractions/) | Abstractions library. |
| `Correlate.AspNetCore` | [![NuGet](https://img.shields.io/nuget/v/Correlate.AspNetCore.svg)](https://www.nuget.org/packages/Correlate.AspNetCore/) [![NuGet](https://img.shields.io/nuget/dt/Correlate.AspNetCore.svg)](https://www.nuget.org/packages/Correlate.AspNetCore/) | ASP.NET Core middleware. |
| `Correlate.DependencyInjection` | [![NuGet](https://img.shields.io/nuget/v/Correlate.DependencyInjection.svg)](https://www.nuget.org/packages/Correlate.DependencyInjection/) [![NuGet](https://img.shields.io/nuget/dt/Correlate.DependencyInjection.svg)](https://www.nuget.org/packages/Correlate.DependencyInjection/) | Extensions for registration in a `IServiceCollection` container. |

## Usage

In a typical ASP.NET Core (MVC) application, register the middleware and required services to handle incoming requests with a correlation id, and to enrich the response with a relevant correlation id.

When using `HttpClient` to call other services, you can use `HttpClientFactory` to attach a delegating handler to any `HttpClient` which will automatically add a correlation id header to the outgoing request for cross service correlation.

### Example ###

Configure your application:

```csharp
using Correlate.AspNetCore;
using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register services.
        services.AddCorrelate(options => 
            options.RequestHeaders = new []
            {
              // List of incoming headers possible. First that is set on given request is used and also returned in the response.
              "X-Correlation-ID",
              "My-Correlation-ID"
            }
        );

        // Register a typed client that will include the correlation id in outgoing request.
        services
            .AddHttpClient<IMyService, MyService>()
            .CorrelateRequests("X-Correlation-ID");

        services.AddMvcCore();
    }

    public void Configure(IApplicationBuilder app)
    {
        // Use middleware.
        app.UseCorrelate();

        app.UseMvc();
    }
}
```

Using this setup, for any incoming request that contains a `X-Correlation-ID` or `My-Correlation-ID` header, the correlation id from that header will be used throughout the request pipeline. For any incoming request without either header, a unique correlation id is generated instead.

Secondly, all responses will receive a matching response header.

Thirdly, for all outbound HTTP calls that are sent via the `HttpClient` provided to `MyService` instances, a `X-Correlation-ID` request header is added.

> In order to capture incoming requests with correlation ids as soon as possible, the middleware should be registered as the first middleware in the pipeline or at least near the top. Otherwise, you have middleware executing outside of the correlation context scope making them untrackable.

## Logging

Before a request flows down the pipeline, a log scope is created with a `CorrelationId` property, containing the correlation id. As such, every log event that is logged during the entire request context will be enriched with the `CorrelationId` property.

Most popular log providers will be able to log the correlation id with minimal set up required.

Here's some providers that require no set up or custom code, only configuration:

- Serilog: `new LoggerConfiguration().Enrich.FromLogContext()`
  https://github.com/serilog/serilog/wiki/Enrichment#the-logcontext  
- NLog: https://github.com/NLog/NLog/wiki/MDLC-Layout-Renderer `${mdlc:item=CorrelationId}`

## ICorrelationContextAccessor - Getting the correlation id from anywhere

To access the correlation id anywhere in code, inject an instance of `ICorrelationContextAccessor` in your constructor. 

### Example

```csharp
public class MyService
{
    public MyService(ICorrelationContextAccessor correlationContextAccessor)
    {
        string correlationId = correlationContextAccessor.CorrelationContext.CorrelationId;
    }
}
```

> Note: `correlationContextAccessor.CorrelationContext` can be null, when `MyService` is not scoped to a request. Thus, when used outside of ASP.NET (not using the middleware component), you should create the context using `CorrelationManager` or `IActivityFactory` respectively (depending on the use case) for each unique subprocess.

## Correlation outside of ASP.NET request context

To simplify managing correlation contexts, the `CorrelationManager` can be used. It takes care of the logic to create the context properly. This is especially useful when running background tasks, console apps, Windows services, etc. which need to interact with external services. Think of message broker handlers, scheduled task runners, etc.

### Example

```csharp
public class MyWorker
{
    private readonly IAsyncCorrelationManager _correlationManager;
    private readonly MyService _myService;

    public MyWorker(IAsyncCorrelationManager correlationManager, MyService myService)
    {
        _correlationManager = correlationManager;
        _myService = myService;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            await _correlationManager.CorrelateAsync(async () => {
                // Do work in a scoped correlation context with its own correlation id.
                await myService.MakeHttpCallAsync();
            });

            await Task.Delay(5000);
        }
    }
}
```

In this example the `MakeHttpCallAsync()` call is executed in a correlation context, for which a correlation id is automatically generated and attached to the outgoing request provided the `HttpClient` is set up with the delegating handler:

```csharp
services
    .AddHttpClient<IMyService, MyService>()
    .CorrelateRequests("X-Correlation-ID");
```

### Providing an existing correlation id

As an example, consider a use case where the order id should be used as a correlation id. The `CorrelationManager` has an overload that accepts a custom correlation id:

```csharp
await _correlationManager.CorrelateAsync(orderId, () => { 
  // Do work
});
```

### Async/sync support

`CorrelationManager` provides both a synchronous and asynchronous implementation, and they can be requested from the service provider independently:

- `ICorrelationManager`
- `IAsyncCorrelationManager`

> Note that the Correlate internals are intrinsically asynchronous as it relies on [`AsyncLocal<T>`](https://docs.microsoft.com/en-us/dotnet/api/system.threading.asynclocal-1) to save the correlation context. The synchronous implementation is useful for integrations that have a synchronous API surface but are still used in asynchronous context, but should only be used as such.

## ICorrelationIdFactory

By default, when generating a correlation id, the `GuidCorrelationIdFactory` will produce guid based correlation ids.

As an alternative, there's also a `RequestIdentifierCorrelationIdFactory` which produces base32 encoded correlation ids. To use this or a custom implementation  instead, simply register it manually in the service container.

### Example
```csharp
services.AddSingleton<ICorrelationIdFactory, RequestIdentifierCorrelationIdFactory>();
```

## Integrations

| Framework/library | Type | Package | Description |
| - | - | - | - |
| [Rebus](https://github.com/rebus-org/Rebus) | Service&#160;bus | [Rebus.Correlate](https://github.com/skwasjer/Rebus.Correlate) | Rebus integration of Correlate to correlate message flow via any supported Rebus transport. |
| [Hangfire](https://www.hangfire.io/) | Job&#160;scheduler | [Hangfire.Correlate](https://github.com/skwasjer/Hangfire.Correlate) | Hangfire integration of Correlate to add correlation id support to Hangfire background/scheduled jobs. |

## Alternatives for more advanced Distributed Tracing

Please consider that .NET Core 3.1 and up now has built-in support for [W3C TraceContext](https://github.com/w3c/trace-context) ([blog](https://devblogs.microsoft.com/aspnet/improvements-in-net-core-3-0-for-troubleshooting-and-monitoring-distributed-apps/)) and that there are other distributed tracing libraries with more functionality than Correlate.

- [OpenTelemetry](https://opentelemetry.io/)
- [Jaeger](https://www.jaegertracing.io/)
- [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

## More info

### Supported .NET targets
- .NET 6.0
- .NET Standard 2.1/.NET Core 3.1

### ASP.NET Core support
- ASP.NET Core 3.1/6.0

### Build requirements
- Visual Studio 2022
- .NET 6 SDK
- .NET 3.1 SDK

#### Contributions
PR's are welcome. Please rebase before submitting, provide test coverage, and ensure the AppVeyor build passes. I will not consider PR's otherwise.

#### Contributors
- skwas (author/maintainer)
