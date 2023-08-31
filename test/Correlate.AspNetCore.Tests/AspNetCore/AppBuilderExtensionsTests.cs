using Correlate.AspNetCore.Middleware;
using Correlate.DependencyInjection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Options;

namespace Correlate.AspNetCore;

[Collection(nameof(UsesDiagnosticListener))]
public sealed class AppBuilderExtensionsTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ApplicationLifetime _appLifetime;

    public AppBuilderExtensionsTests()
    {
        _appLifetime = new ApplicationLifetime(Substitute.For<ILogger<ApplicationLifetime>>());
        _serviceProvider = new ServiceCollection()
            .AddCorrelate()
            .AddSingleton<IHostApplicationLifetime>(_appLifetime)
            .BuildServiceProvider();
    }

    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _serviceProvider?.Dispose();
        _appLifetime?.StopApplication();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }

    [Fact]
    public void Given_that_appBuilder_is_null_when_usingCorrelate_it_should_throw()
    {
        IApplicationBuilder? appBuilder = null;

        // Act
        Func<IApplicationBuilder> act = () => appBuilder!.UseCorrelate();

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithParameterName(nameof(appBuilder));
    }

    [Fact]
    public void Given_that_correlate_is_already_used_when_using_again_it_should_throw()
    {
        IApplicationBuilder appBuilder = new ApplicationBuilder(
            _serviceProvider,
            new FeatureCollection()
        );
        appBuilder.UseCorrelate();

        // Act
        Func<IApplicationBuilder> act = () => appBuilder.UseCorrelate();

        // Assert
        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("* should not be called more than once.");
    }

    [Fact]
    public void Given_that_options_are_not_registered_when_using_it_should_not_throw()
    {
        IServiceCollection services = new ServiceCollection()
            .AddCorrelate()
            .AddSingleton<IHostApplicationLifetime>(_appLifetime);
        services.Should().NotContain(sd => sd.ServiceType == typeof(IOptions<CorrelateOptions>));

        using ServiceProvider? sp = services.BuildServiceProvider();
        IApplicationBuilder appBuilder = new ApplicationBuilder(
            _serviceProvider,
            new FeatureCollection()
        );

        // Act
        Action act = () => appBuilder.UseCorrelate();

        // Assert
        act.Should().NotThrow();
        appBuilder.ApplicationServices.GetService<IOptions<CorrelateOptions>>()
            .Should()
            .NotBeNull();
    }
}
