using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.AspNet.Tests.Fixtures;

internal sealed class ScopedDependencyResolver(IServiceScope scope) : IDependencyScope
{
    public object? GetService(Type serviceType)
    {
        return scope.ServiceProvider.GetService(serviceType);
    }

    public IEnumerable<object?> GetServices(Type serviceType)
    {
        return scope.ServiceProvider.GetServices(serviceType);
    }

    public void Dispose()
    {
        scope.Dispose();
    }
}
