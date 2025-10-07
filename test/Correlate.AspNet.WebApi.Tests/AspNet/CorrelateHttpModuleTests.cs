using System.Web;
using Correlate.DependencyInjection;
using Correlate.Http.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.AspNet;

public sealed class CorrelateHttpModuleTests
{
    private readonly CorrelateHttpModule _sut;
    private readonly Func<DefaultHttpListener> _httpListenerFactory;

    public CorrelateHttpModuleTests()
    {
        _httpListenerFactory = Substitute.For<Func<DefaultHttpListener>>();
        _httpListenerFactory
            .Invoke()
            .Returns(_ =>
            {
                using ServiceProvider services = new ServiceCollection()
                    .AddCorrelate()
                    .BuildServiceProvider();
                using var resolver = new DefaultDependencyResolver(services);
                return resolver.GetHttpListener();
            });
        _sut = new CorrelateHttpModule(_httpListenerFactory);
    }

    [Fact]
    public void When_initializing_twice_it_should_resolve_correlate_services_only_once()
    {
        using var app = new HttpApplication();

        // Act
        ((IHttpModule)_sut).Init(app);

        // Assert
        _httpListenerFactory.Received(1).Invoke();

        // Act 2
        _httpListenerFactory.ClearReceivedCalls();
        ((IHttpModule)_sut).Init(app);

        // Assert 2
        _httpListenerFactory.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public void Given_that_context_is_null_when_initializing_it_should_throw()
    {
        HttpApplication context = null!;

        // Act
        Action act = () => ((IHttpModule)_sut).Init(context);

        // Assert
        act.Should()
            .ThrowExactly<ArgumentNullException>()
            .WithParameterName(nameof(context));
    }
}
