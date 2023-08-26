using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Correlate.Testing.FluentAssertions;

public class ServiceCollectionAssertions : ReferenceTypeAssertions<IServiceCollection, ServiceCollectionAssertions>
{
    public ServiceCollectionAssertions(IServiceCollection actualValue)
        : base(actualValue)
    {
    }

    protected override string Identifier => "ServiceProviderAssertions";

    public AndConstraint<ServiceCollectionAssertions> BeRegistered(ExpectedRegistration registration, string because = "", params object[] becauseArgs)
    {
        return BeRegistered(registration.ServiceType, registration.ImplementationType, registration.Lifetime, because, becauseArgs);
    }

    public AndConstraint<ServiceCollectionAssertions> BeRegistered(Type serviceType, Type implementationType, ServiceLifetime lifetime, string because = "", params object[] becauseArgs)
    {
        AssertionScope scope = Execute.Assertion;
        scope.AddReportable("lifetime", lifetime.ToString().ToLowerInvariant());
        scope
            .BecauseOf(because, becauseArgs)
            .WithExpectation("Expected {context:service collection} to contain a {lifetime} registration for {0} implemented by {1}{reason}, ", serviceType, implementationType)
            // Match service and lifetime
            .Given(() => Subject.Where(d => d.ServiceType == serviceType))
            .ForCondition(reg => reg.Any())
            .FailWith("but the service was not found.")
            .Then
            .ForCondition(reg => reg.Any(d => d.Lifetime == lifetime))
            .FailWith("but found {0}.", reg => reg.Select(d => d.Lifetime).Distinct())
            .Then
            .Given(reg => reg.FirstOrDefault(d => d.Lifetime == lifetime))
            .ForCondition(r =>
                // Match implementation, instance or factory
                r is not null
             && (
                r.ImplementationType == implementationType
             || (r.ImplementationInstance is not null && implementationType.IsInstanceOfType(r.ImplementationInstance))
             || r.ImplementationFactory is not null
                )
            )
            .FailWith("but it does not.")
            ;

        return new AndConstraint<ServiceCollectionAssertions>(this);
    }
}
