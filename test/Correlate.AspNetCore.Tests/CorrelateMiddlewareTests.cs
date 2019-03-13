using System.Collections;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Correlate.AspNetCore.Fixtures;
using Correlate.AspNetCore.FluentAssertions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace Correlate.AspNetCore
{
	public class CorrelateMiddlewareTests : IClassFixture<TestAppFactory<Startup>>
	{
		private readonly WebApplicationFactory<Startup> _factory;
		private readonly CorrelateOptions _options;

		public CorrelateMiddlewareTests(TestAppFactory<Startup> factory)
		{
			_options = new CorrelateOptions();

			_factory = factory.WithWebHostBuilder(builder => builder
				.ConfigureTestServices(services =>
				{
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
		public async Task Given_request_contains_requestId_when_executing_request_the_response_should_contain_header_with_requestId()
		{
			const string headerName = CorrelationHttpHeaders.RequestId;
			const string correlationId = "my-correlation-id";

			HttpClient client = _factory.CreateClient();
			var request = new HttpRequestMessage();
			request.Headers.Add(headerName, correlationId);

			// Act
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

			HttpClient client = _factory.CreateClient();

			// Act
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

			HttpClient client = _factory.CreateClient();

			// Act
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

			HttpClient client = _factory.CreateClient();
			var request = new HttpRequestMessage();
			request.Headers.Add(headerName, correlationId);

			using (TestCorrelator.CreateContext())
			{
				// Act
				await client.SendAsync(request);

				// Assert
				// ReSharper disable once SuggestVarOrType_Elsewhere
				var logEvents = TestCorrelator.GetLogEventsFromCurrentContext().ToList();
				logEvents.Should().HaveCountGreaterThan(2);

				logEvents
					.Take(1)
					.Union(logEvents.TakeLast(1))
					.Should()
					.Contain(le => le.Properties.ContainsKey("CorrelationId") && ((ScalarValue)le.Properties["CorrelationId"]).Value == null, "the first and last log item are logged by the WebHost before the middleware");
				logEvents
					.Skip(1)
					.SkipLast(1)
					.Should()
					.Contain(le => le.Properties.ContainsKey("CorrelationId") && (string)((ScalarValue)le.Properties["CorrelationId"]).Value == correlationId, "all log items except the first and last are logged after the middleware");
			}
		}
	}
}
