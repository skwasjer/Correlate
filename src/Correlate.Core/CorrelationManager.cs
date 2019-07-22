using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Correlate
{
	/// <summary>
	/// The correlation manager runs activities in its own <see cref="CorrelationContext"/> allowing correlation with external services.
	/// </summary>
	public class CorrelationManager : IAsyncCorrelationManager
	{
		private readonly ICorrelationContextFactory _correlationContextFactory;
		private readonly ICorrelationIdFactory _correlationIdFactory;
		private readonly ICorrelationContextAccessor _correlationContextAccessor;
		private readonly ILogger _logger;
		private readonly DiagnosticListener _diagnosticListener;

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationManager"/> class.
		/// </summary>
		/// <param name="correlationContextFactory">The correlation context factory used to create new contexts.</param>
		/// <param name="correlationIdFactory">The correlation id factory used to generate a new correlation id per context.</param>
		/// <param name="logger">The logger.</param>
		[Obsolete("Use the Ctor() that accepts 'ICorrelationContextAccessor'.")]
		public CorrelationManager
		(
			ICorrelationContextFactory correlationContextFactory,
			ICorrelationIdFactory correlationIdFactory,
			ILogger<CorrelationManager> logger
		)
		{
			_correlationContextFactory = correlationContextFactory ?? throw new ArgumentNullException(nameof(correlationContextFactory));
			_correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationManager"/> class.
		/// </summary>
		/// <param name="correlationContextFactory">The correlation context factory used to create new contexts.</param>
		/// <param name="correlationIdFactory">The correlation id factory used to generate a new correlation id per context.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="diagnosticListener">The diagnostics listener to run activities on.</param>
		[Obsolete("Use the Ctor() that accepts 'ICorrelationContextAccessor'.")]
		public CorrelationManager(
			ICorrelationContextFactory correlationContextFactory,
			ICorrelationIdFactory correlationIdFactory,
			ILogger<CorrelationManager> logger,
			DiagnosticListener diagnosticListener
		) : this(correlationContextFactory, correlationIdFactory, logger)
		{
			_diagnosticListener = diagnosticListener ?? throw new ArgumentNullException(nameof(diagnosticListener));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationManager"/> class.
		/// </summary>
		/// <param name="correlationContextFactory">The correlation context factory used to create new contexts.</param>
		/// <param name="correlationIdFactory">The correlation id factory used to generate a new correlation id per context.</param>
		/// <param name="correlationContextAccessor">The correlation context accessor.</param>
		/// <param name="logger">The logger.</param>
		public CorrelationManager
		(
			ICorrelationContextFactory correlationContextFactory,
			ICorrelationIdFactory correlationIdFactory,
			ICorrelationContextAccessor correlationContextAccessor,
			ILogger<CorrelationManager> logger
		)
		{
			_correlationContextFactory = correlationContextFactory ?? throw new ArgumentNullException(nameof(correlationContextFactory));
			_correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
			_correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationManager"/> class.
		/// </summary>
		/// <param name="correlationContextFactory">The correlation context factory used to create new contexts.</param>
		/// <param name="correlationIdFactory">The correlation id factory used to generate a new correlation id per context.</param>
		/// <param name="correlationContextAccessor">The correlation context accessor.</param>
		/// <param name="logger">The logger.</param>
		/// <param name="diagnosticListener">The diagnostics listener to run activities on.</param>
		public CorrelationManager(
			ICorrelationContextFactory correlationContextFactory,
			ICorrelationIdFactory correlationIdFactory,
			ICorrelationContextAccessor correlationContextAccessor,
			ILogger<CorrelationManager> logger,
			DiagnosticListener diagnosticListener
		) : this(correlationContextFactory, correlationIdFactory, correlationContextAccessor, logger)
		{
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
		/// <param name="correlatedTask">The task to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <returns>An awaitable that completes once the <paramref name="correlatedTask"/> has executed and the correlation context has disposed.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
		/// </remarks>
		public Task CorrelateAsync(Func<Task> correlatedTask, OnException onException)
		{
			return CorrelateAsync(null, correlatedTask, onException);
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
			return CorrelateInternalAsync(correlationId, null, correlatedTask, null);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedTask"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedTask">The task to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <returns>An awaitable that completes once the <paramref name="correlatedTask"/> has executed and the correlation context has disposed.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
		/// </remarks>
		public Task CorrelateAsync(string correlationId, Func<Task> correlatedTask, OnException onException)
		{
			return CorrelateInternalAsync(correlationId, null, correlatedTask, onException);
		}

		internal Task CorrelateInternalAsync(string correlationId, IActivity innerActivity, Func<Task> correlatedTask, OnException onException = null)
		{
			if (correlatedTask == null)
			{
				throw new ArgumentNullException(nameof(correlatedTask));
			}

			return ExecuteAsync(
				correlationId,
				new RootActivity(_correlationContextFactory, _logger, _diagnosticListener, innerActivity),
				correlatedTask,
				onException
			);
		}

		private async Task ExecuteAsync(string correlationId, RootActivity activity, Func<Task> correlatedTask, OnException onException)
		{
			CorrelationContext correlationContext = activity.Start(correlationId ?? _correlationContextAccessor?.CorrelationContext?.CorrelationId ?? _correlationIdFactory.Create());

			try
			{
				await correlatedTask().ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				if (correlationContext != null && !ex.Data.Contains(CorrelateConstants.CorrelationIdKey))
				{
					// Because we're about to lose context scope, enrich exception with correlation id for reference by calling code.
					ex.Data.Add(CorrelateConstants.CorrelationIdKey, correlationContext.CorrelationId);
				}

				if (onException == null)
				{
					throw;
				}

				// Allow caller to handle exception inline before losing context scope.
				bool isHandled = onException(correlationContext, ex);
				if (!isHandled)
				{
					throw;
				}
			}
			finally
			{
				activity.Stop();
			}
		}
	}
}