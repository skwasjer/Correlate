using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Correlate.AspNetCore.Middleware
{
	/// <summary>
	/// Middleware that takes correlation id from incoming request header and persists it throughout the request chain in a <see cref="CorrelationContext"/> object.
	/// </summary>
	public class CorrelateMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly CorrelateOptions _options;
		private readonly ILogger<CorrelateMiddleware> _logger;
		private readonly ICorrelationIdFactory _correlationIdFactory;
		private readonly CorrelationManager _correlationManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelateMiddleware"/> class.
		/// </summary>
		/// <param name="next">The next request delegate to invoke in the request execution pipeline.</param>
		/// <param name="options">The options.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="correlationIdFactory">The correlation id factory to create new correlation ids.</param>
		/// <param name="correlationManager">The correlation manager.</param>
		public CorrelateMiddleware(
			RequestDelegate next,
			IOptions<CorrelateOptions> options,
			ILogger<CorrelateMiddleware> logger,
			ICorrelationIdFactory correlationIdFactory,
			CorrelationManager correlationManager)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
			_correlationManager = correlationManager ?? throw new ArgumentNullException(nameof(correlationManager));
		}

		/// <summary>
		/// Invokes the middleware for the current <paramref name="httpContext"/>.
		/// </summary>
		/// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
		/// <returns>An awaitable to wait for to complete the request.</returns>
		public Task Invoke(HttpContext httpContext)
		{
			(string correlationId, string headerName) = GetCorrelationId(httpContext.Request);

			var correlatedHttpRequest = new HttpRequestActivity(httpContext, _options, _logger, headerName);
			return _correlationManager.CorrelateInternalAsync(
				correlationId, 
				correlatedHttpRequest, 
				() => _next(httpContext)
			);
		}

		private (string CorrelationId, string HeaderName) GetCorrelationId(HttpRequest httpRequest)
		{
			string correlationId = null;
			string headerName = null;

			if (_options.RequestHeaders != null)
			{
				foreach (string requestHeaderName in _options.RequestHeaders)
				{
					if (httpRequest.Headers.TryGetValue(requestHeaderName, out StringValues value))
					{
						headerName = requestHeaderName;
						correlationId = value.FirstOrDefault();
						if (!string.IsNullOrEmpty(correlationId))
						{
							_logger.LogDebug("Request header '{HeaderName}' found with correlation id '{CorrelationId}'.", headerName, correlationId);
							break;
						}
					}
				}
			}

			return (
				string.IsNullOrWhiteSpace(correlationId) ? _correlationIdFactory.Create() : correlationId,
				headerName ?? _options.RequestHeaders?.FirstOrDefault() ?? CorrelationHttpHeaders.CorrelationId
			);
		}
	}
}
