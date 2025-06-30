using System.Net;
using System.Net.Http;
using Correlate.AspNet.Options;
using Correlate.AspNet.Tests.Fixtures;
using Correlate.Extensions;
using Correlate.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Microsoft.Owin.Testing;
using MockHttp;

namespace Correlate.AspNet.Tests;

[Collection(nameof(UsesDiagnosticListener))]
public sealed class IntegrationTests : IClassFixture<TestAppFactory<Startup>>, IDisposable
{
    private readonly TestServer _testServer;
    private readonly TestAppFactory<Startup> _rootFactory;
    private readonly CorrelateOptionsNet48 _options;
    private readonly MockHttpHandler _mockHttp;
    private FakeLogCollector LogCollector => _rootFactory.ServiceProvider!.GetFakeLogCollector();

    public IntegrationTests(TestAppFactory<Startup> factory)
    {
        _options = new CorrelateOptionsNet48();
        _mockHttp = new MockHttpHandler();

        _rootFactory = factory;
        _rootFactory.LoggingEnabled = true;

        _testServer = _rootFactory.CreateServer(services =>
        {
            services.AddTransient(_ => _mockHttp);
            services.AddSingleton<IOptions<CorrelateOptionsNet48>>(_ => new OptionsWrapper<CorrelateOptionsNet48>(_options));
        });
    }

    public void Dispose()
    {
        // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
        _testServer?.Dispose();
        _mockHttp?.Dispose();
        // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
    }

    [Fact]
    public async Task Given_custom_header_is_defined_when_executing_request_the_response_should_contain_custom_header()
    {
        const string headerName = "my-header";
        _options.RequestHeaders = [headerName];

        // Act
        HttpClient client = _testServer.HttpClient;
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Unexpected response: {await response.Content.ReadAsStringAsync()}");
        var headerValues = response.Headers.GetValues(headerName).ToList();
        Assert.Single(headerValues);
        Assert.NotEmpty(headerValues.First());
    }

    [Fact]
    public async Task Given_default_configuration_when_executing_request_the_response_should_contain_header_with_correlationId()
    {
        HttpClient client = _testServer.HttpClient;
        const string headerName = "X-Correlation-ID";

        // Act
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Unexpected response: {await response.Content.ReadAsStringAsync()}");
        var headerValues = response.Headers.GetValues(headerName).ToList();
        Assert.Single(headerValues);
        Assert.NotEmpty(headerValues.Single());
    }

    [Fact]
    public async Task Given_no_headers_are_defined_when_executing_request_the_response_should_contain_default_header()
    {
        _options.RequestHeaders = [];
        const string headerName = "X-Correlation-ID";

        // Act
        HttpClient client = _testServer.HttpClient;
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Unexpected response: {await response.Content.ReadAsStringAsync()}");
        var headerValues = response.Headers.GetValues(headerName).ToList();
        Assert.Single(headerValues);
        Assert.NotEmpty(headerValues.Single());
    }

    [Fact]
    public async Task Given_request_contains_correlationId_when_executing_request_the_response_should_contain_header_with_correlationId()
    {
        const string headerName = CorrelationHttpHeaders.CorrelationId;
        const string correlationId = "my-correlation-id";

        var request = new HttpRequestMessage();
        request.Headers.Add(headerName, correlationId);

        // Act
        HttpClient client = _testServer.HttpClient;
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Unexpected response: {await response.Content.ReadAsStringAsync()}");
        var headerValues = response.Headers.GetValues(headerName).ToList();
        Assert.Single(headerValues);
        Assert.NotEmpty(headerValues.Single());
    }

    [Fact]
    public async Task Given_response_is_disabled_when_executing_request_the_response_should_not_contain_correlation_id()
    {
        _options.IncludeInResponse = false;

        // Act
        HttpClient client = _testServer.HttpClient;
        HttpResponseMessage response = await client.GetAsync("");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, $"Unexpected response: {await response.Content.ReadAsStringAsync()}");
        response.Headers.Should().BeEmpty();
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
        HttpClient client = _testServer.HttpClient;
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
        HttpClient client = _testServer.HttpClient;
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
    public async Task When_logging_and_diagnostics_is_disabled_should_not_throw_in_controller_and_not_create_logScope()
    {
        _rootFactory.LoggingEnabled = false;

        // Act
        HttpClient client = _testServer.HttpClient;
        Func<Task> act = () => client.GetAsync("");

        // Assert
        await act.Should().NotThrowAsync<Exception>();

        IReadOnlyList<FakeLogRecord> logEvents = LogCollector.GetSnapshot(true);
        logEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task When_logging_should_have_correlationId_for_all_logged_events()
    {
        const string headerName = CorrelationHttpHeaders.CorrelationId;
        const string correlationId = "my-correlation-id";
    
        var request = new HttpRequestMessage();
        request.Headers.Add(headerName, correlationId);
    
        LogCollector.Clear();
    
        // Act
        HttpClient client = _testServer.HttpClient;
        await client.SendAsync(request);
    
        // Assert
        IReadOnlyList<FakeLogRecord> logEvents = LogCollector.GetSnapshot(true);
        logEvents.Should()
            .HaveCountGreaterThan(1)
            .And.AllSatisfy(ev => ev.Scopes
                .Should()
                .ContainEquivalentOf(new LoggerExtensions.CorrelatedLogScope(CorrelateConstants.CorrelationIdKey, correlationId))
            );
    }
    
    [Theory]
    [MemberData(nameof(GetOptionBindingTestCases))]
    public void Options_should_deserialize_from_config(Dictionary<string, string?> configDict, CorrelateOptionsNet48 expectedOptions)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();
    
        // Act
        using ServiceProvider services = new ServiceCollection()
            .Configure<CorrelateOptionsNet48>(config.Bind)
            .BuildServiceProvider();
    
        // Assert
        CorrelateOptionsNet48 opts = services.GetRequiredService<IOptions<CorrelateOptionsNet48>>().Value;
        opts.Should().BeEquivalentTo(expectedOptions);
    }

    public static IEnumerable<object[]> GetOptionBindingTestCases()
    {
        yield return
        [
            new Dictionary<string, string?>(),
            new CorrelateOptionsNet48
            {
                IncludeInResponse = true,
                LoggingScopeKey = CorrelateConstants.CorrelationIdKey,
                RequestHeaders = null
            }
        ];
        yield return
        [
            new Dictionary<string, string?>
            {
                { "LoggingScopeKey", "LogKey1" },
                { "IncludeInResponse", "false" },
                { "RequestHeaders:0", "Header1" },
                { "RequestHeaders:1", "Header2" }
            },
            new CorrelateOptionsNet48
            {
                LoggingScopeKey = "LogKey1",
                IncludeInResponse = false,
                RequestHeaders = ["Header1", "Header2"]
            }
        ];
    }
}
