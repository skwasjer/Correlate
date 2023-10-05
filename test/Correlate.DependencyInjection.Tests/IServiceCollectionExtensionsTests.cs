﻿using System.Collections;
using Correlate.Testing.FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Correlate.DependencyInjection;

public class When_adding_correlate_to_container
{
    private readonly IServiceCollection _services;
    private readonly IServiceProvider _sut;

    protected When_adding_correlate_to_container(IServiceCollection services)
    {
        _services = services;

        _sut = _services.BuildServiceProvider();
    }

    public When_adding_correlate_to_container()
        : this(new ServiceCollection()
            .AddLogging()
            .AddCorrelate())
    {
    }

    [Theory]
    [ClassData(typeof(ExpectedRegistrations))]
    public void It_should_resolve(ExpectedRegistration registration)
    {
        _sut.Should().Resolve(registration.ServiceType);
    }

    [Theory]
    [ClassData(typeof(ExpectedRegistrations))]
    public void It_should_be_registered(ExpectedRegistration registration)
    {
        _services.Should().BeRegistered(registration);
    }

    public class ExpectedRegistrations : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            return TestCases().Select(tc => new object[] { tc }).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual IEnumerable<ExpectedRegistration> TestCases()
        {
            yield return new ExpectedRegistration<ICorrelationContextAccessor, CorrelationContextAccessor>(ServiceLifetime.Singleton);
            yield return new ExpectedRegistration<ICorrelationContextFactory, CorrelationContextFactory>(ServiceLifetime.Transient);
            yield return new ExpectedRegistration<ICorrelationIdFactory, GuidCorrelationIdFactory>(ServiceLifetime.Singleton);
            yield return new ExpectedRegistration<CorrelationManager, CorrelationManager>(ServiceLifetime.Transient);
            yield return new ExpectedRegistration<IAsyncCorrelationManager, CorrelationManager>(ServiceLifetime.Transient);
            yield return new ExpectedRegistration<ICorrelationManager, CorrelationManager>(ServiceLifetime.Transient);
            yield return new ExpectedRegistration<IActivityFactory, CorrelationManager>(ServiceLifetime.Transient);
            yield return new ExpectedRegistration<ILoggerFactory, LoggerFactory>(ServiceLifetime.Singleton);
        }
    }
}

public class When_adding_correlate_to_container_with_options : When_adding_correlate_to_container
{
    public When_adding_correlate_to_container_with_options()
        : base(new ServiceCollection()
            .AddLogging()
            .AddCorrelate(o => o.LoggingScopeKey = "CustomLoggingScopeKey"))
    {
    }
}