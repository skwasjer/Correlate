using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Correlate.Testing.FluentAssertions;

public class ServiceProviderAssertions : ReferenceTypeAssertions<IServiceProvider, ServiceProviderAssertions>
{
    public ServiceProviderAssertions(IServiceProvider actualValue)
        : base(actualValue)
    {
    }

    protected override string Identifier => "ServiceProviderAssertions";

    public AndConstraint<ServiceProviderAssertions> Resolve<TService>(string because = "", params object[] becauseArgs)
        where TService : class
    {
        return Resolve(typeof(TService), because, becauseArgs);
    }

    public AndConstraint<ServiceProviderAssertions> Resolve(Type serviceType, string because = "", params object[] becauseArgs)
    {
        AssertionScope scope = Execute.Assertion;
        scope
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected to resolve {0} from {context:service provider}{reason}, ", serviceType)
            .Given(() =>
            {
                try
                {
                    return Subject.GetService(serviceType);
                }
                catch (Exception ex)
                {
                    return ex;
                }
            })
            .ForCondition(value => value is not Exception)
            .FailWith("but threw with {0}.", ex => ex)
            .Then
            .ForCondition(value => value is not null)
            .FailWith("but failed.")
            ;

        return new AndConstraint<ServiceProviderAssertions>(this);
    }
}
