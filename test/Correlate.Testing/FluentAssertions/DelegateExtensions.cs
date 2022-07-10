using FluentAssertions;

namespace Correlate.Testing.FluentAssertions;

public static class DelegateExtensions
{
    public static DelegateAssertions Should(this Delegate instance)
    {
        return new DelegateAssertions(instance, new AggregateExceptionExtractor());
    }
}
