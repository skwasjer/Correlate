using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Correlate
{
	internal class RootActivity : IActivity
	{
		private readonly ICorrelationContextFactory _correlationContextFactory;
		private readonly ILogger _logger;
		private readonly DiagnosticListener _diagnosticListener;
		private readonly IActivity _activity;
		private IDisposable _logScope;

		public RootActivity(
			ICorrelationContextFactory correlationContextFactory,
			ILogger logger,
			DiagnosticListener diagnosticListener,
			IActivity activity)
		{
			_correlationContextFactory = correlationContextFactory ?? throw new ArgumentNullException(nameof(correlationContextFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_diagnosticListener = diagnosticListener ?? throw new ArgumentNullException(nameof(diagnosticListener));
			_activity = activity;
		}

		/// <summary>
		/// Starts the correlation context.
		/// </summary>
		/// <param name="correlationId">The correlation id to create the context for.</param>
		/// <returns>The created correlation context (also accessible via <see cref="ICorrelationContextAccessor"/>), or null if diagnostics and logging is disabled.</returns>
		public CorrelationContext Start(string correlationId)
		{
			if (correlationId == null)
			{
				throw new ArgumentNullException(nameof(correlationId));
			}

			bool isDiagnosticsEnabled = _diagnosticListener.IsEnabled();
			bool isLoggingEnabled = _logger.IsEnabled(LogLevel.Critical);

			if (isDiagnosticsEnabled || isLoggingEnabled)
			{
				CorrelationContext context = _correlationContextFactory.Create(correlationId);

				if (isDiagnosticsEnabled)
				{
					// TODO: add Activity support
					//var activity = new Activity("Correlated-Request");
					//activity.SetParentId(correlationId);
					//_diagnosticListener.StartActivity(activity, new {})
				}

				if (isLoggingEnabled)
				{
					_logScope = _logger.BeginCorrelatedScope(correlationId);
				}

				_activity?.Start(context);

				return context;
			}

			return null;
		}

		void IActivity.Start(CorrelationContext correlationContext)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Stops the correlation context.
		/// </summary>
		public void Stop()
		{
			_activity?.Stop();

			//_diagnosticListener.StopActivity(activity, new {})
			_logScope?.Dispose();
			_correlationContextFactory.Dispose();
		}
	}
}