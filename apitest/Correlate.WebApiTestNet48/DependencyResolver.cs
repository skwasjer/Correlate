using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.WebApiTestNet48;

public class DependencyResolver(IServiceProvider serviceProvider) : IDependencyResolver
{
    private IServiceProvider ServiceProvider { get; } = serviceProvider;

    public IDependencyScope BeginScope() => new DependencyResolver(ServiceProvider.CreateScope().ServiceProvider);

    public void Dispose() => (ServiceProvider as IDisposable)?.Dispose();

    public object GetService(Type serviceType) => ServiceProvider.GetService(serviceType);

    public IEnumerable<object?> GetServices(Type serviceType) => ServiceProvider.GetServices(serviceType);
}
