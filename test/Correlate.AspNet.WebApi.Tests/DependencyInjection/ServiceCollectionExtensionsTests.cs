using Correlate.AspNet;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Correlate.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCorrelateNet48_WithoutConfiguration_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCorrelate();

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        IActivityFactory? service = provider.GetService<IActivityFactory>();
        Assert.NotNull(service);

        IOptions<CorrelateOptions>? options = provider.GetService<IOptions<CorrelateOptions>>();
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
        services.AddCorrelate(options =>
        {
            options.LoggingScopeKey = expectedLoggingScopeKey;
            options.RequestHeaders = [expectedHeaderName];
            options.IncludeInResponse = false;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();

        IOptions<CorrelateOptions>? options = provider.GetService<IOptions<CorrelateOptions>>();
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
        Action<CorrelateOptions>? config = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddCorrelate(config!));
    }

    [Fact]
    public void AddCorrelateNet48_MultipleRegistrations_DoesNotThrowException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddCorrelate();
        services.AddCorrelate(); // Second registration

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IActivityFactory? correlateFeature = provider.GetService<IActivityFactory>();
        Assert.NotNull(correlateFeature);
    }

    [Fact]
    public void AddCorrelateNet48_ConfigurationOverrides_LastConfigurationWins()
    {
        // Arrange
        var services = new ServiceCollection();

        var expectedOptions = new CorrelateOptions
        {
            IncludeInResponse = false,
            RequestHeaders = ["Second"],
            LoggingScopeKey = "SecondKey",
        };

        // Act
        services.AddCorrelate(options =>
        {
            options.IncludeInResponse = true;
            options.RequestHeaders = ["First"];
            options.LoggingScopeKey = "FirstKey";
        });
        services.AddCorrelate(options =>
        {
            options.IncludeInResponse = expectedOptions.IncludeInResponse;
            options.RequestHeaders = expectedOptions.RequestHeaders;
            options.LoggingScopeKey = expectedOptions.LoggingScopeKey;
        });

        // Assert
        ServiceProvider provider = services.BuildServiceProvider();
        IOptions<CorrelateOptions>? options = provider.GetService<IOptions<CorrelateOptions>>();
        Assert.NotNull(options);
        Assert.Equivalent(options.Value, expectedOptions);
    }
}
