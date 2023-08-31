using System.Net;
using System.Net.Http.Headers;
using Correlate.AspNetCore.Fixtures;
using Correlate.Http;
using Correlate.Testing.FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Options;
using MockHttp;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace Correlate.AspNetCore;

[Collection(nameof(UsesDiagnosticListener))]
public sealed class IntegrationTests : IClassFixture<TestAppFactory<Startup>>, IDisposable
{
    private readonly WebApplicationFactory<Startup> _factory;
    private readonly TestAppFactory<Startup> _rootFactory;
    private readonly CorrelateOptions _options;
    private readonly MockHttpHandler _mockHttp;

    public IntegrationTests(TestAppFactory<Startup> factory)
    {
        _options = new CorrelateOptions();
        _mockHttp = new MockHttpHandler();

        _rootFactory = factory;
        _rootFactory.LoggingEnabled = true;

        _factory = factory.WithWebHostBuilder(builder => builder
            .ConfigureTestServices(services =>
            {
                services.AddTransient(_ => _mockHttp);
                services.AddSingleton(Options.Create(_options));
            })
        );
    }

    public void Dispose()
    {
#if NETCOREAPP3_1
        // NET Core 3.1 test host does not trigger application stopping token, but our diagnostics observable
        // (subscribed in UseCorrelate) must unsubscribe before next test run.
        // This is a bit crude but later versions of .NET test host do it properly.
        // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        IHostApplicationLifetime? appLifetime = _factory?.Services.GetRequiredService<IHostApplicationLifetime>();
        appLifetime?.StopApplication();
#endif
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _factory?.Dispose();
        _mockHttp?.Dispose();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }

    [Fact]
    public async Task Given_custom_header_is_defined_when_executing_request_the_response_should_contain_custom_header()
    {
        const string headerName = "my-header";
        _options.RequestHeaders = new[] { headerName };

        // Act
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.Headers
            .Should()
            .ContainCorrelationId(headerName)
            .WhichValue.Should()
            .ContainSingle()
            .Which.Should()
            .NotBeEmpty();
    }

    [Fact]
    public async Task Given_default_configuration_when_executing_request_the_response_should_contain_header_with_correlationId()
    {
        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.Headers
            .Should()
            .ContainCorrelationId()
            .WhichValue.Should()
            .ContainSingle()
            .Which.Should()
            .NotBeEmpty();
    }

    [Fact]
    public async Task Given_no_headers_are_defined_when_executing_request_the_response_should_contain_default_header()
    {
        _options.RequestHeaders = Array.Empty<string>();

        // Act
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.Headers
            .Should()
            .ContainCorrelationId()
            .WhichValue.Should()
            .ContainSingle()
            .Which.Should()
            .NotBeEmpty();
    }

    [Fact]
    public async Task Given_request_contains_correlationId_when_executing_request_the_response_should_contain_header_with_correlationId()
    {
        const string headerName = CorrelationHttpHeaders.CorrelationId;
        const string correlationId = "my-correlation-id";

        var request = new HttpRequestMessage();
        request.Headers.Add(headerName, correlationId);

        // Act
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.Headers
            .Should()
            .ContainCorrelationId()
            .WhichValue.Should()
            .BeEquivalentTo(correlationId);
    }

    [Fact]
    public async Task Given_response_is_disabled_when_executing_request_the_response_should_not_contain_correlation_id()
    {
        _options.IncludeInResponse = false;

        // Act
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        ((HttpHeaders)response.Headers).Should().BeEmpty();
    }

    [Fact]
    public async Task When_calling_external_service_in_microservice_should_forward_correlationId()
    {
        const string headerName = CorrelationHttpHeaders.CorrelationId;
        const string correlationId = "my-correlation-id";

        var request = new HttpRequestMessage(HttpMethod.Get, "correlate_client_request");
        request.Headers.Add(headerName, correlationId);

        _mockHttp
            .When(matching => matching
                .RequestUri("*/correlated_external_call")
                .Headers($"{headerName}: {correlationId}")
            )
            .Respond(with =>
                with.StatusCode(HttpStatusCode.Accepted)
            )
            .Verifiable();

        // Act
        HttpClient client = _factory.CreateClient();
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        string? errorMessage = null;
        if (!response.IsSuccessStatusCode)
        {
            errorMessage = await response.Content.ReadAsStringAsync();
        }

        errorMessage.Should().BeNullOrEmpty();
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        _mockHttp.Verify();
        _mockHttp.VerifyNoOtherRequests();
    }

    [Fact]
    public async Task When_executing_multiple_requests_the_response_should_contain_new_correlationIds_for_each_response()
    {
        HttpClient client = _factory.CreateClient();
        const int requestCount = 50;
        IEnumerable<Task<HttpResponseMessage>> requestTasks = Enumerable.Range(0, requestCount)
            .Select(_ => client.GetAsync(""));

        // Act
        var responses = (await Task.WhenAll(requestTasks)).ToList();

        // Assert
        string?[] correlationIds = responses
            .Select(r => r.Headers.SingleOrDefault(h => h.Key == CorrelationHttpHeaders.CorrelationId).Value.FirstOrDefault())
            .ToArray();

        var distinctCorrelationIds = correlationIds.ToHashSet();
        correlationIds
            .Should()
            .HaveCount(requestCount)
            .And
            .BeEquivalentTo(distinctCorrelationIds, "each request should have a different correlation id");
    }

    [Fact]
    public async Task When_logging_and_diagnostics_is_disabled_should_not_throw_in_controller()
    {
        _rootFactory.LoggingEnabled = false;

        // Act
        HttpClient client = _factory.CreateClient();
        Func<Task> act = () => client.GetAsync("");

        // Assert
        await act.Should().NotThrowAsync<Exception>();
    }

    [Fact]
    public async Task When_logging_should_have_correlationId_for_all_logged_events()
    {
        const string headerName = CorrelationHttpHeaders.CorrelationId;
        const string correlationId = "my-correlation-id";

        var request = new HttpRequestMessage();
        request.Headers.Add(headerName, correlationId);

        // Act
        HttpClient client = _factory.CreateClient();
        await client.SendAsync(request);

        // Assert
        // ReSharper disable once SuggestVarOrType_Elsewhere
        var logEvents = TestCorrelator.GetLogEventsFromContextGuid(Startup.LastRequestContext!.Guid).ToList();
        logEvents.Should().HaveCountGreaterThan(2);

        logEvents
            .ToList()
            .ForEach(le => le.Properties
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                .Should()
                .ContainKey(CorrelateConstants.CorrelationIdKey)
                .WhoseValue.Should()
                .BeOfType<ScalarValue>()
                .Which.Value.Should()
                .Be(correlationId));
    }
}
