An ASP.NET (classic/.net framework) implementation to correlate activities between decoupled components (eg. microservices) via a HTTP header.

This package uses an IIS module to automatically handle the correlation of HTTP requests and responses. To activate the module, you need to add it to your web.config file.

```xml
  <system.webServer>
    <validation validateIntegratedModeConfiguration="false" />
    <modules>
      <add name="CorrelateHttpModule" type="Correlate.AspNet.CorrelateHttpModule, Correlate.AspNet" />
    </modules>
  </system.webServer>
  <system.web>
    <httpModules>
      <add name="CorrelateHttpModule" type="Correlate.AspNet.CorrelateHttpModule, Correlate.AspNet" />
    </httpModules>
  </system.web>
```

If you already have a `<modules>/<httpModules>` section in your web.config, you can simply just add the line with `CorrelateHttpModule`. If you don't have one yet, you can copy the above snippet into your web.config file.

### Useful links

- [GitHub / docs](https://github.com/skwasjer/Correlate)
- [Changelog](https://github.com/skwasjer/Correlate/releases)
- [Examples](https://github.com/skwasjer/Correlate/tree/main/examples)
