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
		private readonly CorrelateOptions _options;
		private IDisposable _logScope;

		internal HttpRequestActivity(HttpContext httpContext, CorrelateOptions options, ILogger logger, string responseHeaderName)
		{
			_httpContext = httpContext;
			_options = options;
			_logger = logger;
			_responseHeaderName = responseHeaderName;
		}

		public void Start(CorrelationContext correlationContext)
		{
			bool isLoggingEnabled = _logger.IsEnabled(LogLevel.Critical);
			if (isLoggingEnabled)
			{
				_logScope = _logger.BeginRequestScope(_httpContext, correlationContext.CorrelationId);
			}

			if (_options.IncludeInResponse)
			{
				_httpContext.Response.OnStarting(() =>
				{
					// If already set, ignore.
					if (!_httpContext.Response.Headers.ContainsKey(_responseHeaderName))
					{
						_httpContext.Response.Headers.Add(_responseHeaderName, correlationContext.CorrelationId);
					}

					return Task.CompletedTask;
				});
			}
		}

		public void Stop()
		{
			_logScope?.Dispose();
		}
	}
}