using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.AspNet.Tests.Fixtures;

internal class DefaultDependencyResolver(IServiceProvider serviceProvider) : IDependencyResolver
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
        return new ScopedDependencyResolver(serviceProvider.CreateScope());
    }

    public void Dispose()
    {
        (serviceProvider as IDisposable)?.Dispose();
    }
}
