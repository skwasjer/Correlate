using Correlate.AspNet.Extensions;
using Correlate.AspNet.Middlewares;
using Correlate.AspNet.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Correlate.AspNet.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCorrelateNet48_WithoutConfiguration_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCorrelateNet48();

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
            
        ICorrelateFeatureNet48? correlateFeature = provider.GetService<ICorrelateFeatureNet48>();
        Assert.NotNull(correlateFeature);
        Assert.IsType<CorrelateFeatureNet48>(correlateFeature);

        IOptions<CorrelateOptionsNet48>? options = provider.GetService<IOptions<CorrelateOptionsNet48>>();
        Assert.NotNull(options);
    }

    [Fact]
    public void AddCorrelateNet48_WithConfiguration_AppliesConfigurationCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        const string expectedLoggingScopeKey = "CustomKey";
        const string expectedHeaderName = "Custom-Correlation-Id";

        // Act
        services.AddCorrelateNet48(options =>
        {
            options.LoggingScopeKey = expectedLoggingScopeKey;
            options.RequestHeaders = [expectedHeaderName];
            options.IncludeInResponse = false;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
            
        ICorrelateFeatureNet48? correlateFeature = provider.GetService<ICorrelateFeatureNet48>();
        Assert.NotNull(correlateFeature);

        IOptions<CorrelateOptionsNet48>? options = provider.GetService<IOptions<CorrelateOptionsNet48>>();
        Assert.NotNull(options);
        Assert.Equal(expectedLoggingScopeKey, options.Value.LoggingScopeKey);
        Assert.Contains(expectedHeaderName, options.Value.RequestHeaders!);
        Assert.False(options.Value.IncludeInResponse);
    }

    [Fact]
    public void AddCorrelateNet48_WithNullConfiguration_ThrowsArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        Action<CorrelateOptionsNet48>? config = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddCorrelateNet48(config!));
    }

    [Fact]
    public void AddCorrelateNet48_MultipleRegistrations_DoesNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCorrelateNet48();
        services.AddCorrelateNet48(); // Second registration

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        ICorrelateFeatureNet48? correlateFeature = provider.GetService<ICorrelateFeatureNet48>();
        Assert.NotNull(correlateFeature);
    }

    [Fact]
    public void AddCorrelateNet48_ConfigurationOverrides_LastConfigurationWins()
    {
        // Arrange
        var services = new ServiceCollection();
        const string firstKey = "FirstKey";
        const string secondKey = "SecondKey";

        // Act
        services.AddCorrelateNet48(options => options.LoggingScopeKey = firstKey);
        services.AddCorrelateNet48(options => options.LoggingScopeKey = secondKey);

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IOptions<CorrelateOptionsNet48>? options = provider.GetService<IOptions<CorrelateOptionsNet48>>();
        Assert.NotNull(options);
        Assert.Equal(secondKey, options.Value.LoggingScopeKey);
    }
}
