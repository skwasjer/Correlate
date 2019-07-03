using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Correlate.AspNetCore.Middleware
{
	internal class HttpRequestActivity : IActivity
	{
		private readonly HttpContext _httpContext;
		private readonly ILogger _logger;
		private readonly string _responseHeaderName;
		private readonly bool _includeInResponse;

		internal HttpRequestActivity(ILogger logger, HttpContext httpContext, string responseHeaderName)
		{
			_logger = logger;
			_httpContext = httpContext;
			_responseHeaderName = responseHeaderName;
			_includeInResponse = !string.IsNullOrWhiteSpace(responseHeaderName);
		}

		public void Start(CorrelationContext correlationContext)
		{
			if (_includeInResponse)
			{
				_httpContext.Response.OnStarting(() =>
				{
					// If already set, ignore.
					if (!_httpContext.Response.Headers.ContainsKey(_responseHeaderName))
					{
						_logger.LogTrace("Setting response header '{HeaderName}' to correlation id '{CorrelationId}'.", _responseHeaderName, correlationContext.CorrelationId);
						_httpContext.Response.Headers.Add(_responseHeaderName, correlationContext.CorrelationId);
					}

					return Task.CompletedTask;
				});
			}
		}

		public void Stop()
		{
		}
	}
}