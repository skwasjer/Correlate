using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.DependencyInjection;

/// <summary>
/// A default implementation of <see cref="IDependencyResolver"/> that uses <see cref="IServiceProvider"/> to resolve dependencies.
/// </summary>
/// <remarks>
/// This class is designed to bridge the dependency resolution capabilities of <see cref="Microsoft.Extensions.DependencyInjection"/> with the Web API dependency resolver.
/// </remarks>
public sealed class DefaultDependencyResolver(IServiceProvider serviceProvider) : IDependencyResolver
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IServiceScope? _scope;

    private DefaultDependencyResolver(IServiceScope scope)
        : this(scope.ServiceProvider)
    {
        _scope = scope;
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType)
    {
        return _serviceProvider.GetService(serviceType);
    }

    /// <inheritdoc />
    public IEnumerable<object?> GetServices(Type serviceType)
    {
        return _serviceProvider.GetServices(serviceType);
    }

    /// <inheritdoc />
    public IDependencyScope BeginScope()
    {
        return new DefaultDependencyResolver(_serviceProvider.CreateScope());
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_scope is not null)
        {
            _scope.Dispose();
            return;
        }

        (_serviceProvider as IDisposable)?.Dispose();
    }
}
