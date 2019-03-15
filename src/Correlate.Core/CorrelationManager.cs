using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Correlate
{
	/// <summary>
	/// The correlation manager runs activities in its own <see cref="CorrelationContext"/> allowing correlation with external services.
	/// </summary>
	public class CorrelationManager
	{
		private readonly ICorrelationContextFactory _correlationContextFactory;
		private readonly ICorrelationIdFactory _correlationIdFactory;
		private readonly ILogger _logger;
		private readonly DiagnosticListener _diagnosticListener;

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationManager"/> class.
		/// </summary>
		/// <param name="correlationContextFactory">The correlation context factory used to create new contexts.</param>
		/// <param name="correlationIdFactory">The correlation id factory used to generate a new correlation id per context.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="diagnosticListener">The diagnostics listener to run activities on.</param>
		public CorrelationManager(
			ICorrelationContextFactory correlationContextFactory,
			ICorrelationIdFactory correlationIdFactory,
			ILogger<CorrelationManager> logger,
			DiagnosticListener diagnosticListener
		)
		{
			_correlationContextFactory = correlationContextFactory ?? throw new ArgumentNullException(nameof(correlationContextFactory));
			_correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_diagnosticListener = diagnosticListener ?? throw new ArgumentNullException(nameof(diagnosticListener));
		}

		/// <summary>
		/// Executes the <paramref name="correlatedTask"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlatedTask">The task to execute.</param>
		/// <returns>An awaitable that completes once the <paramref name="correlatedTask"/> has executed and the correlation context has disposed.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
		/// </remarks>
		public Task CorrelateAsync(Func<Task> correlatedTask)
		{
			return CorrelateAsync(null, correlatedTask);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedTask"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedTask">The task to execute.</param>
		/// <returns>An awaitable that completes once the <paramref name="correlatedTask"/> has executed and the correlation context has disposed.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
		/// </remarks>
		public Task CorrelateAsync(string correlationId, Func<Task> correlatedTask)
		{
			return CorrelateInternalAsync(correlationId, null, correlatedTask);
		}

		internal async Task CorrelateInternalAsync(string correlationId, IActivity innerActivity, Func<Task> correlatedTask)
		{
			if (correlatedTask == null)
			{
				throw new ArgumentNullException(nameof(correlatedTask));
			}

			var correlation = new RootActivity(_correlationContextFactory, _logger, _diagnosticListener, innerActivity);

			correlation.Start(correlationId ?? _correlationIdFactory.Create());

			try
			{
				await correlatedTask();
			}
			finally
			{
				correlation.Stop();
			}
		}
	}
}