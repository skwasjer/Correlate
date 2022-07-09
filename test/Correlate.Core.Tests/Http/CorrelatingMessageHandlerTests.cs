﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MockHttp;
using Xunit;

namespace Correlate.Http
{
	public sealed class CorrelatingMessageHandlerTests : IDisposable
	{
		private readonly CorrelationContextAccessor _contextAccessor;
		private readonly CorrelatingHttpMessageHandler _sut;
		private readonly HttpClient _httpClient;
		private readonly MockHttpHandler _mockHttp;
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

			_mockHttp = new MockHttpHandler();

			_sut = new CorrelatingHttpMessageHandler(_contextAccessor, new OptionsWrapper<CorrelateClientOptions>(_correlateClientOptions), _mockHttp);
			_httpClient = new HttpClient(_sut)
			{
				BaseAddress = BaseUri
			};
		}

		[Fact]
		public async Task Given_a_correlation_context_should_add()
		{
			_contextAccessor.CorrelationContext?.CorrelationId.Should().NotBeNull();
			string correlationId = _contextAccessor.CorrelationContext!.CorrelationId;

			_mockHttp
				.When(matching => matching
					.RequestUri(BaseUri + "*")
					.Method(HttpMethod.Get)
					.Headers($"{_correlateClientOptions.RequestHeader}: {correlationId}")
				)
				.Respond(with => with
					.StatusCode(HttpStatusCode.OK)
				)
				.Verifiable();

			// Act
			await _httpClient.GetAsync("");

			// Assert
			_mockHttp.Verify();
			_mockHttp.VerifyNoOtherRequests();
		}

		[Fact]
		public async Task Given_no_correlation_context_should_not_add()
		{
			_contextAccessor.CorrelationContext = null;

			_mockHttp
				.When(matching => matching
					.RequestUri(BaseUri + "*")
					.Method(HttpMethod.Get)
					.Where(message => !message.Headers.Any())
				)
				.Respond(with => with
					.StatusCode(HttpStatusCode.OK)
				)
				.Verifiable();

			// Act
			await _httpClient.GetAsync("");

			// Assert
			_mockHttp.Verify();
			_mockHttp.VerifyNoOtherRequests();
		}

		[Fact]
		public async Task Given_correlationId_is_already_in_request_headers_should_not_overwrite()
		{
			const string existingCorrelationId = "existing-correlation-id";

			_mockHttp
				.When(matching => matching
					.RequestUri(BaseUri + "*")
					.Method(HttpMethod.Get)
					.Headers($"{_correlateClientOptions.RequestHeader}: {existingCorrelationId}")
				)
				.Respond(with => with
					.StatusCode(HttpStatusCode.OK)
				)
				.Verifiable();

			// Act
			await _httpClient.SendAsync(new HttpRequestMessage
			{
				Headers = { { _correlateClientOptions.RequestHeader, existingCorrelationId } }
			});

			// Assert
			_mockHttp.Verify();
			_mockHttp.VerifyNoOtherRequests();
		}

		[Fact]
		public async Task Given_headerName_is_overridden_should_not_use_default_headerName()
		{
			_correlateClientOptions.RequestHeader = "custom-header";
			_contextAccessor.CorrelationContext?.CorrelationId.Should().NotBeNull();
			string correlationId = _contextAccessor.CorrelationContext!.CorrelationId;

			_mockHttp
				.When(matching => matching
					.RequestUri(BaseUri + "*")
					.Method(HttpMethod.Get)
					.Headers($"{_correlateClientOptions.RequestHeader}: {correlationId}")
				)
				.Respond(with => with
					.StatusCode(HttpStatusCode.OK)
				)
				.Verifiable();

			// Act
			await _httpClient.GetAsync("");

			// Assert
			_mockHttp.Verify();
			_mockHttp.VerifyNoOtherRequests();
		}

		public void Dispose()
		{
			_sut?.Dispose();
			_httpClient?.Dispose();
			_mockHttp?.Dispose();
		}
	}
}
