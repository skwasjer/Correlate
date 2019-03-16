using System;
using System.Threading.Tasks;
using Correlate.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
		private readonly CorrelationManager _correlationManager;

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelateMiddleware"/> class.
		/// </summary>
		/// <param name="next">The next request delegate to invoke in the request execution pipeline.</param>
		/// <param name="options">The options.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="correlationManager">The correlation manager.</param>
		public CorrelateMiddleware(
			RequestDelegate next,
			IOptions<CorrelateOptions> options,
			ILogger<CorrelateMiddleware> logger,
			CorrelationManager correlationManager)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_correlationManager = correlationManager ?? throw new ArgumentNullException(nameof(correlationManager));
		}

		/// <summary>
		/// Invokes the middleware for the current <paramref name="httpContext"/>.
		/// </summary>
		/// <param name="httpContext">The current <see cref="HttpContext"/>.</param>
		/// <returns>An awaitable to wait for to complete the request.</returns>
		public Task Invoke(HttpContext httpContext)
		{
			var header = httpContext.Request.GetCorrelationIdHeader(_options.RequestHeaders);
			if (header.Value != null)
			{
				_logger.LogTrace("Request header '{HeaderName}' found with correlation id '{CorrelationId}'.", header.Key, header.Value);
			}

			var correlatedHttpRequest = new HttpRequestActivity(httpContext, _options, _logger, header.Key);
			return _correlationManager.CorrelateInternalAsync(
				header.Value,
				correlatedHttpRequest, 
				() => _next(httpContext)
			);
		}
	}
}
