using Microsoft.Extensions.DependencyInjection;

namespace Correlate.Testing.FluentAssertions;

public static class ShouldExtensions
{
    public static ServiceProviderAssertions Should(this IServiceProvider actualValue)
    {
        return new ServiceProviderAssertions(actualValue);
    }

    public static ServiceCollectionAssertions Should(this IServiceCollection actualValue)
    {
        return new ServiceCollectionAssertions(actualValue);
    }
}
