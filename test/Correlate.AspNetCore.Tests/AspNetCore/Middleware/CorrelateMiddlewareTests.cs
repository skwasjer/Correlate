using System;
using System.Collections;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Correlate.AspNetCore.Fixtures;
using Correlate.FluentAssertions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace Correlate.AspNetCore.Middleware
{
	public class CorrelateMiddlewareTests : IClassFixture<TestAppFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _factory;
		private readonly TestAppFactory<Startup> _rootFactory;
		private readonly CorrelateOptions _options;
		private readonly MockHttpMessageHandler _mockHttp;

		public CorrelateMiddlewareTests(TestAppFactory<Startup> factory)
		{
			_options = new CorrelateOptions();
			_mockHttp = new MockHttpMessageHandler();

			_rootFactory = factory;
			_rootFactory.LoggingEnabled = true;

			_factory = factory.WithWebHostBuilder(builder => builder
				.ConfigureTestServices(services =>
				{
					services.AddTransient(_ => _mockHttp);
					services.AddSingleton<IOptions<CorrelateOptions>>(new OptionsWrapper<CorrelateOptions>(_options));
				})
			);
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
				.Should().ContainCorrelationId(headerName)
				.WhichValue.Should().BeEquivalentTo(correlationId);
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
				.Should().ContainCorrelationId(headerName)
				.WhichValue.Should()
				.ContainSingle()
				.Which.Should().NotBeEmpty();
		}

		[Fact]
		public async Task Given_response_is_disabled_when_executing_request_the_response_should_not_contain_correlation_id()
		{
			_options.IncludeInResponse = false;

			// Act
			HttpClient client = _factory.CreateClient();
			HttpResponseMessage response = await client.GetAsync("");

			// Assert
			response.Headers
				.OfType<IEnumerable>()
				.Should().BeEmpty();
		}

		[Fact]
		public async Task When_logging_should_have_correlationId_for_all_logged_events_except_host_start_and_finish()
		{
			const string headerName = CorrelationHttpHeaders.CorrelationId;
			const string correlationId = "my-correlation-id";

			var request = new HttpRequestMessage();
			request.Headers.Add(headerName, correlationId);

			using (TestCorrelator.CreateContext())
			{
				// Act
				HttpClient client = _factory.CreateClient();
				await client.SendAsync(request);

				// Assert
				// ReSharper disable once SuggestVarOrType_Elsewhere
				var logEvents = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
				logEvents.Should().HaveCountGreaterThan(2);

				logEvents
					.Take(1)
					.Union(logEvents.TakeLast(1))
					.ToList()
					.ForEach(le => le.Properties
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
						.Should().ContainKey("CorrelationId")
						.WhichValue.Should().BeOfType<ScalarValue>()
						.Which.Value.Should().BeNull());
				logEvents
					.Skip(1)
					.SkipLast(1)
					.ToList()
					.ForEach(le => le.Properties
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
						.Should().ContainKey("CorrelationId")
						.WhichValue.Should().BeOfType<ScalarValue>()
						.Which.Value.Should().Be(correlationId));
			}
		}

		[Fact]
		public async Task When_calling_external_service_in_microservice_should_forward_correlationId()
		{
			const string headerName = CorrelationHttpHeaders.CorrelationId;
			const string correlationId = "my-correlation-id";

			var request = new HttpRequestMessage(HttpMethod.Get, "correlate_client_request");
			request.Headers.Add(headerName, correlationId);

			_mockHttp
				.Expect("/correlated_external_call")
				.WithHeaders($"{headerName}: {correlationId}")
				.Respond(HttpStatusCode.Accepted);

			// Act
			HttpClient client = _factory.CreateClient();
			HttpResponseMessage response = await client.SendAsync(request);

			// Assert
			string errorMessage = null;
			if (!response.IsSuccessStatusCode)
			{
				errorMessage = await response.Content.ReadAsStringAsync();
			}

			errorMessage.Should().BeNullOrEmpty();
			response.StatusCode.Should().Be(HttpStatusCode.Accepted);

			_mockHttp.VerifyNoOutstandingExpectation();
		}

		[Fact]
		public void When_logging_and_diagnostics_is_disabled_should_throw_in_controller()
		{
			_rootFactory.LoggingEnabled = false;

			// Act
			HttpClient client = _factory.CreateClient();
			Func<Task> act = () => client.GetAsync("");

			// Assert
			act.Should().Throw<NullReferenceException>()
				.Which.StackTrace
				.Should()
				.StartWith("   at Correlate.AspNetCore.Fixtures.TestController.Get() ");
		}
	}
}
