﻿using System.Net;
using Correlate.Http;
using Correlate.Testing.FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using MockHttp;

namespace Correlate.DependencyInjection;

public class When_adding_correlation_delegating_handler_to_httpClient
{
    private readonly IServiceCollection _services;
    private readonly IHttpClientBuilder _sut;
    private readonly MockHttpHandler _mockHttp;

    private class MyService
    {
        public MyService(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }
    }

    [Theory]
    [ClassData(typeof(ExpectedRegistrations))]
    public void It_should_resolve(ExpectedRegistration registration)
    {
        // Act
        _sut.CorrelateRequests();
        IServiceProvider actual = _services.BuildServiceProvider();

        // Assert
        actual.Should().Resolve(registration.ServiceType);
    }

    [Theory]
    [ClassData(typeof(ExpectedRegistrations))]
    public void It_should_be_registered(ExpectedRegistration registration)
    {
        // Act
        _sut.CorrelateRequests();

        // Assert
        _services.Should().BeRegistered(registration);
    }

    public class ExpectedRegistrations : When_adding_correlate_to_container.ExpectedRegistrations
    {
        protected override IEnumerable<ExpectedRegistration> TestCases()
        {
            foreach (ExpectedRegistration testCase in base.TestCases())
            {
                yield return testCase;
            }

            yield return new ExpectedRegistration<CorrelatingHttpMessageHandler, CorrelatingHttpMessageHandler>(ServiceLifetime.Transient);

            // AddHttpMessageHandler with options:
            yield return new ExpectedRegistration<IConfigureOptions<CorrelateClientOptions>, ConfigureNamedOptions<CorrelateClientOptions>>(ServiceLifetime.Singleton);
            yield return new ExpectedRegistration<IConfigureOptions<HttpClientFactoryOptions>>(ServiceLifetime.Singleton);
            yield return new ExpectedRegistration<MyService>(ServiceLifetime.Transient);
        }
    }

    public When_adding_correlation_delegating_handler_to_httpClient()
    {
        _mockHttp = new MockHttpHandler();
        _services = new ServiceCollection();
        _sut = _services
            .AddHttpClient<MyService, MyService>()
            .ConfigurePrimaryHttpMessageHandler(() => _mockHttp);
    }

    [Fact]
    public async Task When_providing_different_headerName_the_typed_service_client_should_use_this_headerName()
    {
        var expectedOptions = new CorrelateClientOptions { RequestHeader = "override-header" };

        _mockHttp
            .When(matching => matching
                .RequestUri("*/test/")
                .Where(message => message.Headers.Contains(expectedOptions.RequestHeader))
            )
            .Respond(with =>
                with.StatusCode(HttpStatusCode.OK)
            )
            .Verifiable();

        _sut.CorrelateRequests(expectedOptions.RequestHeader);
        IServiceProvider services = _services.BuildServiceProvider();
        MyService service = services.GetRequiredService<MyService>();
        IAsyncCorrelationManager asyncCorrelationManager = services.GetRequiredService<IAsyncCorrelationManager>();

        // Act
        await asyncCorrelationManager.CorrelateAsync(async () =>
        {
            // Act
            await service.HttpClient.GetAsync("http://0.0.0.0/test/");
        });

        // Assert
        _mockHttp.Verify();
        _mockHttp.VerifyNoOtherRequests();
        services.GetRequiredService<IOptions<CorrelateClientOptions>>()
            .Value
            .RequestHeader.Should()
            .NotBe(expectedOptions.RequestHeader, "the client options are specific to a typed client");
    }
}
