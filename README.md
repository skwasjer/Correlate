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

[![Main workflow](https://github.com/skwasjer/Correlate/actions/workflows/main.yml/badge.svg)](https://github.com/skwasjer/Correlate/actions/workflows/main.yml)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=skwasjer_Correlate&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=skwasjer_Correlate)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=skwasjer_Correlate&metric=coverage)](https://sonarcloud.io/component_measures?id=skwasjer_Correlate&metric=coverage)

| | | |
|---|---|---|
| `Correlate` | [![NuGet](https://img.shields.io/nuget/v/Correlate.svg)](https://www.nuget.org/packages/Correlate/) [![NuGet](https://img.shields.io/nuget/dt/Correlate.svg)](https://www.nuget.org/packages/Correlate/) | Core library, including a `DelegatingHandler` for `HttpClient`. |
| `Correlate.Abstractions` | [![NuGet](https://img.shields.io/nuget/v/Correlate.Abstractions.svg)](https://www.nuget.org/packages/Correlate.Abstractions/) [![NuGet](https://img.shields.io/nuget/dt/Correlate.Abstractions.svg)](https://www.nuget.org/packages/Correlate.Abstractions/) | Abstractions library. |
| `Correlate.AspNetCore` | [![NuGet](https://img.shields.io/nuget/v/Correlate.AspNetCore.svg)](https://www.nuget.org/packages/Correlate.AspNetCore/) [![NuGet](https://img.shields.io/nuget/dt/Correlate.AspNetCore.svg)](https://www.nuget.org/packages/Correlate.AspNetCore/) | ASP.NET Core integration. |
| `Correlate.DependencyInjection` | [![NuGet](https://img.shields.io/nuget/v/Correlate.DependencyInjection.svg)](https://www.nuget.org/packages/Correlate.DependencyInjection/) [![NuGet](https://img.shields.io/nuget/dt/Correlate.DependencyInjection.svg)](https://www.nuget.org/packages/Correlate.DependencyInjection/) | Extensions for registration in a `IServiceCollection` container. |

## Usage

In an ASP.NET Core (MVC) application, register Correlate to handle incoming requests with a correlation id. Correlate will create a request scoped async context holding the correlation id, which can then propagate down the request pipeline.

When using `HttpClient` to call other services, you can use `HttpClientFactory` to attach a delegating handler to any `HttpClient` which will propagate the correlation id header to the outgoing request for cross service correlation. Further more, there are other integration packages that also propagate the correlation id to other transports (see down below).

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
        // Registers a diagnostics request observer.
        app.UseCorrelate();

        app.UseMvc();
    }
}
```

Using this setup, for any incoming request that contains a `X-Correlation-ID` or `My-Correlation-ID` header, the correlation id from that header will be used throughout the request pipeline. For any incoming request without either header, a unique correlation id is generated instead.

Secondly, all responses will receive a matching response header.

Thirdly, for all outbound HTTP calls that are sent via the `HttpClient` provided to `MyService` instances, a `X-Correlation-ID` request header is added.

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

> Note: `correlationContextAccessor.CorrelationContext` can be null, when `MyService` is not scoped to a request. Thus, when used outside of ASP.NET (not using the middleware component), you should create the context using `I(Async)CorrelationManager` or `IActivityFactory` respectively (depending on the use case) for each unique subprocess.

## Correlation outside of ASP.NET request context

To simplify managing correlation contexts, the `I(Async)CorrelationManager` can be used. It takes care of the logic to create the context properly. This is especially useful when running background tasks, console apps, Windows services, etc. which need to interact with external services. Think of message broker handlers, scheduled task runners, etc.

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

Please consider that since .NET Core 3.1 and up there is built-in support for [W3C TraceContext](https://github.com/w3c/trace-context) ([blog](https://devblogs.microsoft.com/aspnet/improvements-in-net-core-3-0-for-troubleshooting-and-monitoring-distributed-apps/)) and that there are other distributed tracing libraries with more functionality than Correlate. Personally, I am using `System.Diagnostics.ActivitySource` and OpenTelemetry in my professional work.

- [OpenTelemetry](https://opentelemetry.io/)
- [Jaeger](https://www.jaegertracing.io/)
- [Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)

## More info

### Supported .NET targets
- .NET 9.0, .NET 8.0
- .NET Standard 2.0

### ASP.NET Core support
- ASP.NET Core 9.0/8.0

### Build/test requirements
- Visual Studio 2022
- .NET 9 SDK
- .NET 8 SDK
- .NET 3.1 SDK

#### Contributions
PR's are welcome. Please rebase before submitting, provide test coverage, and ensure the AppVeyor build passes. I will not consider PR's otherwise.

#### Contributors
- skwas (author/maintainer)
