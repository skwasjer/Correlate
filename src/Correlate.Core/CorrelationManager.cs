using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Correlate
{
	/// <summary>
	/// The correlation manager runs activities in its own <see cref="CorrelationContext"/> allowing correlation with external services.
	/// </summary>
	public class CorrelationManager : IAsyncCorrelationManager, ICorrelationManager, IActivityFactory
	{
		private readonly ICorrelationContextFactory _correlationContextFactory;
		private readonly ICorrelationIdFactory _correlationIdFactory;
		private readonly ICorrelationContextAccessor? _correlationContextAccessor;
		private readonly ILogger _logger;
		private readonly DiagnosticListener? _diagnosticListener;

		private class Void
		{
			private Void()
			{
			}

			public static readonly Void Null = new Void();
		}

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
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedTask">The task to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <returns>An awaitable that completes once the <paramref name="correlatedTask"/> has executed and the correlation context has disposed.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
		/// </remarks>
		public Task CorrelateAsync(string? correlationId, Func<Task> correlatedTask, OnException? onException)
		{
			if (correlatedTask == null)
			{
				throw new ArgumentNullException(nameof(correlatedTask));
			}

			return ExecuteAsync(
				correlationId,
				async () =>
				{
					await correlatedTask().ConfigureAwait(false);
					return Void.Null;
				},
				onException
			);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedTask"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <typeparam name="T">The return type of the awaitable task.</typeparam>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedTask">The task to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <returns>An awaitable that completes with a result <typeparamref name="T"/> once the <paramref name="correlatedTask"/> has executed and the correlation context has disposed.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
		/// </remarks>
		public Task<T> CorrelateAsync<T>(string? correlationId, Func<Task<T>> correlatedTask, OnException<T>? onException)
		{
			if (correlatedTask == null)
			{
				throw new ArgumentNullException(nameof(correlatedTask));
			}

			return ExecuteAsync(
				correlationId,
				correlatedTask,
				onException == null ? (OnException?)null: context => onException!((ExceptionContext<T>)context)
			);
		}

		private async Task<T> ExecuteAsync<T>(string? correlationId, Func<Task<T>> correlatedTask, OnException? onException)
		{
			IActivity activity = CreateActivity();
			CorrelationContext correlationContext = StartActivity(correlationId, activity);

			try
			{
				return await correlatedTask().ConfigureAwait(false);
			}
			catch (Exception ex) when (HandlesException(onException, correlationContext, ex, out T exceptionResult))
			{
				return exceptionResult;
			}
			finally
			{
				activity.Stop();
			}
		}

		/// <summary>
		/// Executes the <paramref name="correlatedAction"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedAction">The action to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public void Correlate(string? correlationId, Action correlatedAction, OnException? onException)
		{
			if (correlatedAction == null)
			{
				throw new ArgumentNullException(nameof(correlatedAction));
			}

			Execute(correlationId,
				() =>
				{
					correlatedAction();
					return Void.Null;
				},
				onException);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedFunc"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <typeparam name="T">The return type.</typeparam>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedFunc">The func to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <returns>Returns the result of the <paramref name="correlatedFunc"/>.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public T Correlate<T>(string? correlationId, Func<T> correlatedFunc, OnException<T>? onException)
		{
			if (correlatedFunc == null)
			{
				throw new ArgumentNullException(nameof(correlatedFunc));
			}

			return Execute(
				correlationId,
				correlatedFunc,
				onException == null ? (OnException?)null : context => onException!((ExceptionContext<T>)context)
			);
		}

		private T Execute<T>(string? correlationId, Func<T> correlatedFunc, OnException? onException)
		{
			IActivity activity = CreateActivity();
			CorrelationContext correlationContext = StartActivity(correlationId, activity);

			try
			{
				return correlatedFunc();
			}
			catch (Exception ex) when (HandlesException(onException, correlationContext, ex, out T exceptionResult))
			{
				return exceptionResult;
			}
			finally
			{
				activity.Stop();
			}
		}

		/// <summary>
		/// Creates a new activity that can be started and stopped manually.
		/// </summary>
		/// <returns>The correlated activity.</returns>
		public IActivity CreateActivity()
		{
			return new RootActivity(_correlationContextFactory, _logger, _diagnosticListener);
		}

		private static bool HandlesException<T>(OnException? onException, CorrelationContext correlationContext, Exception ex, out T result)
		{
			if (!ex.Data.Contains(CorrelateConstants.CorrelationIdKey))
			{
				// Because we're about to lose context scope, enrich exception with correlation id for reference by calling code.
				ex.Data.Add(CorrelateConstants.CorrelationIdKey, correlationContext.CorrelationId);
			}

			if (onException != null)
			{
				bool hasResultValue = typeof(T) != typeof(Void);

				// Allow caller to handle exception inline before losing context scope.
				ExceptionContext exceptionContext = hasResultValue ? new ExceptionContext<T>() : new ExceptionContext();
				exceptionContext.Exception = ex;
				exceptionContext.CorrelationContext = correlationContext;

				onException(exceptionContext);
				if (exceptionContext.IsExceptionHandled)
				{
					result = hasResultValue
						? ((ExceptionContext<T>)exceptionContext).Result
						: default!;
					return true;
				}
			}

			result = default!;
			return false;
		}

		private CorrelationContext StartActivity(string? correlationId, IActivity activity)
		{
			return activity.Start(correlationId ?? _correlationContextAccessor?.CorrelationContext?.CorrelationId ?? _correlationIdFactory.Create());
		}
	}
}
