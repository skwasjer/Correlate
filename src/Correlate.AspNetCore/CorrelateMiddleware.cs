using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Correlate.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Correlate.AspNetCore
{
	/// <summary>
	/// Middleware that takes correlation id from incoming request header and persists it throughout the request chain in a <see cref="CorrelationContext"/> object.
	/// </summary>
	public class CorrelateMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<CorrelateMiddleware> _logger;
		private readonly DiagnosticListener _diagnosticListener;
		private readonly ICorrelationIdFactory _correlationIdFactory;
		private readonly CorrelateOptions _options;

		public CorrelateMiddleware(RequestDelegate next, IOptions<CorrelateOptions> options, ILogger<CorrelateMiddleware> logger, DiagnosticListener diagnosticListener, ICorrelationIdFactory correlationIdFactory)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_diagnosticListener = diagnosticListener ?? throw new ArgumentNullException(nameof(diagnosticListener));
			_correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
		}

		/// <summary>
		/// Invokes the middleware for the current <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The current <see cref="HttpContext"/>.</param>
		/// <param name="correlationContextFactory">The <see cref="ICorrelationContextFactory"/> used to create a <see cref="CorrelationContext"/> for the current request chain.</param>
		/// <returns>An awaitable to wait for to complete the request.</returns>
		public async Task Invoke(HttpContext context, ICorrelationContextFactory correlationContextFactory)
		{
			string correlationId = null;
			string headerName = null;

			bool diagnosticListenerEnabled = _diagnosticListener.IsEnabled();
			bool loggingEnabled = _logger.IsEnabled(LogLevel.Critical);

			IDisposable logScope = null;
			if (diagnosticListenerEnabled || loggingEnabled)
			{
				(correlationId, headerName) = GetCorrelationId(context.Request);
				correlationContextFactory.Create(correlationId);

				if (diagnosticListenerEnabled)
				{
					//// TODO: abstract to ICorrelatedActivityProvider?
					//var activity = new Activity("Correlated-Request");
					//activity.SetParentId(correlationId);
					//DiagnosticListener dl;
					//dl.IsEnabled()
					//dl.StartActivity(activity, new
					//{
					//});
					//dl.StopActivity();
				}

				if (loggingEnabled)
				{
					logScope = _logger.BeginScope("should be dictionary with traceid, request path and correlation id");
				}

				if (_options.IncludeInResponse)
				{
					context.Response.OnStarting(() =>
					{
						if (!context.Response.Headers.ContainsKey(headerName))
						{
							context.Response.Headers.Add(headerName, correlationId);
						}

						return Task.CompletedTask;
					});
				}

				context.Response.OnCompleted(() =>
				{
	//				activity.Stop();

					correlationContextFactory.Dispose();
					logScope?.Dispose();

					return Task.CompletedTask;
				});
			}

			await _next(context);
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
						if (string.IsNullOrEmpty(correlationId))
						{
							continue;
						}

						_logger.LogDebug("Setting correlation id to '{CorrelationId}' from request header {HeaderName}.", correlationId, headerName);
						break;
					}
				}
			}

			return (
				string.IsNullOrWhiteSpace(correlationId) ? _correlationIdFactory.Create() : correlationId,
				headerName ?? _options.RequestHeaders?.FirstOrDefault() ?? CorrelateOptions.DefaultHeaderName
			);
		}
	}
}
