using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace WebApiTestNet48;

public class DefaultDependencyResolver(IServiceProvider serviceProvider) : IDependencyResolver
{
    public object? GetService(Type serviceType)
    {
        return serviceProvider.GetService(serviceType);
    }

    public IEnumerable<object?> GetServices(Type serviceType)
    {
        return serviceProvider.GetServices(serviceType);
    }

    public IDependencyScope BeginScope()
    {
        return new DefaultDependencyResolver(serviceProvider.CreateScope().ServiceProvider);
    }

    public void Dispose()
    {
        (serviceProvider as IDisposable)?.Dispose();
    }
}
