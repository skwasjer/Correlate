using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Correlate;

/// <summary>
/// The correlation manager runs activities in its own <see cref="CorrelationContext" /> allowing correlation with external services.
/// </summary>
public class CorrelationManager : IAsyncCorrelationManager, ICorrelationManager, IActivityFactory
{
    private readonly ICorrelationContextAccessor? _correlationContextAccessor;
    private readonly ICorrelationContextFactory _correlationContextFactory;
    private readonly ICorrelationIdFactory _correlationIdFactory;
    private readonly DiagnosticListener? _diagnosticListener;
    private readonly ILogger _logger;
    private readonly CorrelationManagerOptions _options = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationManager" /> class.
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
    /// Initializes a new instance of the <see cref="CorrelationManager" /> class.
    /// </summary>
    /// <param name="correlationContextFactory">The correlation context factory used to create new contexts.</param>
    /// <param name="correlationIdFactory">The correlation id factory used to generate a new correlation id per context.</param>
    /// <param name="correlationContextAccessor">The correlation context accessor.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="diagnosticListener">The diagnostics listener to run activities on.</param>
    public CorrelationManager
    (
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
    /// Initializes a new instance of the <see cref="CorrelationManager" /> class.
    /// </summary>
    /// <param name="correlationContextFactory">The correlation context factory used to create new contexts.</param>
    /// <param name="correlationIdFactory">The correlation id factory used to generate a new correlation id per context.</param>
    /// <param name="correlationContextAccessor">The correlation context accessor.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="diagnosticListener">The diagnostics listener to run activities on.</param>
    /// <param name="options">The configuration options.</param>
    public CorrelationManager
    (
        ICorrelationContextFactory correlationContextFactory,
        ICorrelationIdFactory correlationIdFactory,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<CorrelationManager> logger,
        DiagnosticListener diagnosticListener,
        IOptions<CorrelationManagerOptions> options
    ) : this(correlationContextFactory, correlationIdFactory, correlationContextAccessor, logger, diagnosticListener)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Creates a new activity that can be started and stopped manually.
    /// </summary>
    /// <returns>The correlated activity.</returns>
    public IActivity CreateActivity()
    {
        return new RootActivity(_correlationContextFactory, _logger, _diagnosticListener, _options);
    }

    /// <summary>
    /// Executes the <paramref name="correlatedTask" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <param name="correlatedTask">The task to execute.</param>
    /// <param name="onError">A delegate to handle the error inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the error handled, or <see langword="false" /> to throw.</param>
    /// <returns>An awaitable that completes once the <paramref name="correlatedTask" /> has executed and the correlation context has disposed.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
    /// </remarks>
    public Task CorrelateAsync(string? correlationId, Func<Task> correlatedTask, OnError? onError)
    {
        if (correlatedTask is null)
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
            onError
        );
    }

    /// <summary>
    /// Executes the <paramref name="correlatedTask" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <typeparam name="T">The return type of the awaitable task.</typeparam>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <param name="correlatedTask">The task to execute.</param>
    /// <param name="onError">A delegate to handle the error inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the error handled, or <see langword="false" /> to throw.</param>
    /// <returns>An awaitable that completes with a result <typeparamref name="T" /> once the <paramref name="correlatedTask" /> has executed and the correlation context has disposed.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
    /// </remarks>
    public Task<T> CorrelateAsync<T>(string? correlationId, Func<Task<T>> correlatedTask, OnError<T>? onError)
    {
        if (correlatedTask is null)
        {
            throw new ArgumentNullException(nameof(correlatedTask));
        }

        return ExecuteAsync(
            correlationId,
            correlatedTask,
            onError is null
                ? null
                : context => onError((ErrorContext<T>)context)
        );
    }

    /// <summary>
    /// Executes the <paramref name="correlatedAction" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <param name="correlatedAction">The action to execute.</param>
    /// <param name="onError">A delegate to handle the error inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the error handled, or <see langword="false" /> to throw.</param>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
    /// </remarks>
    public void Correlate(string? correlationId, Action correlatedAction, OnError? onError)
    {
        if (correlatedAction is null)
        {
            throw new ArgumentNullException(nameof(correlatedAction));
        }

        Execute(correlationId,
            () =>
            {
                correlatedAction();
                return Void.Null;
            },
            onError);
    }

    /// <summary>
    /// Executes the <paramref name="correlatedFunc" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <param name="correlatedFunc">The func to execute.</param>
    /// <param name="onError">A delegate to handle the error inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the error handled, or <see langword="false" /> to throw.</param>
    /// <returns>Returns the result of the <paramref name="correlatedFunc" />.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
    /// </remarks>
    public T Correlate<T>(string? correlationId, Func<T> correlatedFunc, OnError<T>? onError)
    {
        if (correlatedFunc is null)
        {
            throw new ArgumentNullException(nameof(correlatedFunc));
        }

        return Execute(
            correlationId,
            correlatedFunc,
            onError is null
                ? null
                : context => onError((ErrorContext<T>)context)
        );
    }

    private async Task<T> ExecuteAsync<T>(string? correlationId, Func<Task<T>> correlatedTask, OnError? onError)
    {
        IActivity activity = CreateActivity();
        CorrelationContext correlationContext = StartActivity(correlationId, activity);

        try
        {
            return await correlatedTask().ConfigureAwait(false);
        }
        catch (Exception ex) when (HandlesException(onError, correlationContext, ex, out T exceptionResult))
        {
            return exceptionResult;
        }
        finally
        {
            activity.Stop();
        }
    }

    private T Execute<T>(string? correlationId, Func<T> correlatedFunc, OnError? onError)
    {
        IActivity activity = CreateActivity();
        CorrelationContext correlationContext = StartActivity(correlationId, activity);

        try
        {
            return correlatedFunc();
        }
        catch (Exception ex) when (HandlesException(onError, correlationContext, ex, out T exceptionResult))
        {
            return exceptionResult;
        }
        finally
        {
            activity.Stop();
        }
    }

    private static bool HandlesException<T>(OnError? onError, CorrelationContext correlationContext, Exception ex, out T result)
    {
        if (!ex.Data.Contains(CorrelateConstants.CorrelationIdKey))
        {
            // Because we're about to lose context scope, enrich exception with correlation id for reference by calling code.
            ex.Data.Add(CorrelateConstants.CorrelationIdKey, correlationContext.CorrelationId);
        }

        if (onError is not null)
        {
            bool hasResultValue = typeof(T) != typeof(Void);

            // Allow caller to handle exception inline before losing context scope.
            ErrorContext errorContext = hasResultValue
                ? new ErrorContext<T>(correlationContext, ex)
                : new ErrorContext(correlationContext, ex);

            onError(errorContext);
            if (errorContext.IsErrorHandled)
            {
                result = hasResultValue
                    ? ((ErrorContext<T>)errorContext).Result
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

    private sealed class Void
    {
        public static readonly Void Null = new();

        private Void()
        {
        }
    }
}
