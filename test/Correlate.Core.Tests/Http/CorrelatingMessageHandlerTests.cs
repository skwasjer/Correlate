using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RichardSzalay.MockHttp;
using Xunit;

namespace Correlate.Http
{
	public class CorrelatingMessageHandlerTests : IDisposable
	{
		private readonly CorrelationContextAccessor _contextAccessor;
		private readonly CorrelatingHttpMessageHandler _sut;
		private readonly HttpClient _httpClient;
		private readonly MockHttpMessageHandler _mockHttp;
		private readonly CorrelateClientOptions _correlateClientOptions = new CorrelateClientOptions();

		private static readonly Uri BaseUri = new Uri("http://0.0.0.0/");

		public CorrelatingMessageHandlerTests()
		{
			_contextAccessor = new CorrelationContextAccessor
			{
				CorrelationContext = new CorrelationContext
				{
					CorrelationId = Guid.NewGuid().ToString()
				}
			};

			_mockHttp = new MockHttpMessageHandler();

			_sut = new CorrelatingHttpMessageHandler(_contextAccessor, new OptionsWrapper<CorrelateClientOptions>(_correlateClientOptions), _mockHttp);
			_httpClient = new HttpClient(_sut)
			{
				BaseAddress = BaseUri
			};
		}

		[Fact]
		public async Task Given_a_correlation_context_should_add()
		{
			string correlationId = _contextAccessor.CorrelationContext.CorrelationId;

			_mockHttp
				.Expect(HttpMethod.Get, BaseUri + "*")
				.WithHeaders($"{_correlateClientOptions.RequestHeader}: {correlationId}")
				.Respond(HttpStatusCode.OK);

			// Act
			await _httpClient.GetAsync("");

			// Assert
			_mockHttp.VerifyNoOutstandingExpectation();
		}

		[Fact]
		public async Task Given_no_correlation_context_should_not_add()
		{
			_contextAccessor.CorrelationContext = null;

			_mockHttp
				.Expect(HttpMethod.Get, BaseUri + "*")
				.With(message => !message.Headers.Any())
				.Respond(HttpStatusCode.OK);

			// Act
			await _httpClient.GetAsync("");

			// Assert
			_mockHttp.VerifyNoOutstandingExpectation();
		}

		[Fact]
		public async Task Given_correlationId_is_already_in_request_headers_should_not_overwrite()
		{
			const string existingCorrelationId = "existing-correlation-id";

			_mockHttp
				.Expect(HttpMethod.Get, BaseUri + "*")
				.WithHeaders($"{_correlateClientOptions.RequestHeader}: {existingCorrelationId}")
				.Respond(HttpStatusCode.OK);

			// Act
			await _httpClient.SendAsync(new HttpRequestMessage
			{
				Headers = { { _correlateClientOptions.RequestHeader, existingCorrelationId } }
			});

			// Assert
			_mockHttp.VerifyNoOutstandingExpectation();
		}

		[Fact]
		public async Task Given_headerName_is_overridden_should_not_use_default_headerName()
		{
			_correlateClientOptions.RequestHeader = "custom-header";
			string correlationId = _contextAccessor.CorrelationContext.CorrelationId;

			_mockHttp
				.Expect(HttpMethod.Get, BaseUri + "*")
				.WithHeaders($"{_correlateClientOptions.RequestHeader}: {correlationId}")
				.Respond(HttpStatusCode.OK);

			// Act
			await _httpClient.GetAsync("");

			// Assert
			_mockHttp.VerifyNoOutstandingExpectation();
		}

		public void Dispose()
		{
			_sut?.Dispose();
			_httpClient?.Dispose();
			_mockHttp?.Dispose();
		}
	}
}
