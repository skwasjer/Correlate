Dependency injection extensions to register the Correlate services.

### Dependency registration example

```csharp
var services = new ServiceCollection();
services.AddCorrelate();
```

### HttpClient delegating handler example

```csharp
var services = new ServiceCollection();
services
    .AddHttpClient<MyService>()
    .CorrelateRequests();
```

### Useful links

- [GitHub / docs](https://github.com/skwasjer/Correlate)
- [Changelog](https://github.com/skwasjer/Correlate/releases)
- [Examples](https://github.com/skwasjer/Correlate/tree/main/examples)
