using System.Web.Http.Dependencies;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.DependencyInjection;

public sealed class DefaultDependencyResolverTests
{
    private readonly IServiceScopeFactory _serviceScopeFactoryMock;
    private readonly IServiceProvider _servicesMock;

    public DefaultDependencyResolverTests()
    {
        _serviceScopeFactoryMock = Substitute.For<IServiceScopeFactory>();

        _servicesMock = Substitute.For<IServiceProvider, IDisposable>();
        _servicesMock
            .GetService(typeof(IServiceScopeFactory))
            .Returns(_serviceScopeFactoryMock);
    }

    [Fact]
    public void When_creating_with_null_serviceProvider_it_should_throw()
    {
        IServiceProvider? serviceProvider = null;

        // Act
        Func<DefaultDependencyResolver> act = () => new DefaultDependencyResolver(serviceProvider!);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName(nameof(serviceProvider));
    }

    [Fact]
    public void When_resolving_service_it_should_resolve_with_serviceProvider()
    {
        object instance = new();
        _servicesMock.GetService(typeof(object)).Returns(instance);

        // Act
        using var sut = new DefaultDependencyResolver(_servicesMock);
        object? actual = sut.GetService(typeof(object));

        // Assert
        actual.Should().BeSameAs(instance);
        _servicesMock.Received(1).GetService(typeof(object));
    }

    [Fact]
    public void When_resolving_services_it_should_resolve_with_serviceProvider()
    {
        object[] instances = [new(), new(), new()];
        _servicesMock.GetService(typeof(IEnumerable<object>)).Returns(instances);

        // Act
        using var sut = new DefaultDependencyResolver(_servicesMock);
        IEnumerable<object?> actual = sut.GetServices(typeof(object));

        // Assert
        actual.SequenceEqual(instances).Should().BeTrue();
        _servicesMock.Received(1).GetService(typeof(IEnumerable<object>));
    }

    [Fact]
    public void When_creating_scope_it_should_return_scoped_resolver()
    {
        // Act
        using var sut = new DefaultDependencyResolver(_servicesMock);
        using IDependencyScope actual = sut.BeginScope();

        // Assert
        actual.Should()
            .NotBeNull()
            .And.BeOfType<DefaultDependencyResolver>()
            .Which.Should()
            .NotBeSameAs(sut);
    }

    [Fact]
    public void When_disposing_scope_it_should_dispose_underlying_scope()
    {
        IServiceScope underlyingScope;

        // Act
        using var sut = new DefaultDependencyResolver(_servicesMock);
        using (sut.BeginScope())
        {
            underlyingScope = _serviceScopeFactoryMock.Received(1).CreateScope();
        }

        // Assert
        underlyingScope.Received(1).Dispose();
    }

    [Fact]
    public void Given_that_provider_implements_disposable_when_disposing_it_should_dispose_underlying_provider()
    {
        // Act
        new DefaultDependencyResolver(_servicesMock).Dispose();

        // Assert
        ((IDisposable)_servicesMock).Received(1).Dispose();
    }

    [Fact]
    public void Given_that_provider_does_not_implement_disposable_when_disposing_it_should_not_throw()
    {
        IServiceProvider servicesMock = Substitute.For<IServiceProvider>();

        // Act
        Action act = () => new DefaultDependencyResolver(servicesMock).Dispose();

        // Assert
        act.Should().NotThrow();
    }
}
