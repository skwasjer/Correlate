using Correlate.Testing.Specs;

namespace Correlate.DependencyInjection;

public sealed class PublicApiTests : PublicApiSpec
{
    public PublicApiTests()
        : base(typeof(IServiceCollectionExtensions))
    {
    }
}
