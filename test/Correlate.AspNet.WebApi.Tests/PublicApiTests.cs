using Correlate.AspNet;
using Correlate.Testing.Specs;

namespace Correlate;

public sealed class PublicApiTests : PublicApiSpec
{
    public PublicApiTests()
        : base(typeof(CorrelateHttpModule))
    {
    }
}
