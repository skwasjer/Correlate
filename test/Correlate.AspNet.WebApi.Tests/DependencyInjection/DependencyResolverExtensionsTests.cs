using System.Reflection;
using System.Web.Http.Dependencies;
using Correlate.Http.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.DependencyInjection;

public sealed class DependencyResolverExtensionsTests
{
    [Fact]
    public void Give_that_correlate_services_are_registered_when_resolving_listener_it_should_return_instance()
    {
        using ServiceProvider services = new ServiceCollection()
            .AddCorrelate()
            .BuildServiceProvider();
        using var resolver = new DefaultDependencyResolver(services);

        // Act
        // ReSharper disable once AccessToDisposedClosure
        Func<DefaultHttpListener> act = () => resolver.GetHttpListener();

        // Assert
        act.Should().NotThrow().Which.Should().NotBeNull();
    }

    [Fact]
    public void Give_that_correlate_services_are_not_registered_when_resolving_listener_it_should_throw()
    {
        using ServiceProvider services = new ServiceCollection()
            .BuildServiceProvider();
        using var resolver = new DefaultDependencyResolver(services);
        string? serviceType = typeof(DefaultHttpListener).FullName;

        // Act
        // ReSharper disable once AccessToDisposedClosure
        Func<DefaultHttpListener> act = () => resolver.GetHttpListener();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Correlate was unable to resolve its dependencies. Please configure IDependencyResolver to use Microsoft.Extensions.DependencyInjection or similar.")
            .WithInnerException<InvalidOperationException>()
            .WithMessage($"Unable to resolve service for type * while attempting to activate '{serviceType}'*");
    }

    [Fact]
    public void Given_that_resolver_is_default_empty_when_resolving_listener_it_should_throw()
    {
        IDependencyResolver emptyResolver = typeof(IDependencyResolver).Assembly
                .GetTypes()
                .SingleOrDefault(t => t.FullName == "System.Web.Http.Dependencies.EmptyResolver")
                ?.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public)
                ?.GetValue(null) as IDependencyResolver
         ?? throw new InvalidOperationException("Can't get empty resolver.");

        Func<DefaultHttpListener> act = () => emptyResolver.GetHttpListener();

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Correlate was unable to resolve its dependencies. Please configure IDependencyResolver to use Microsoft.Extensions.DependencyInjection or similar.");
    }
}
