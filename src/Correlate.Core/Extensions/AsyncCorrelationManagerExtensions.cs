

// ReSharper disable once CheckNamespace - Justification: common extension methods for the manager should be readily accessible.
namespace Correlate;

/// <summary>
/// Extensions for <see cref="IAsyncCorrelationManager" />.
/// </summary>
public static class AsyncCorrelationManagerExtensions
{
    /// <summary>
    /// Executes the <paramref name="correlatedTask" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <param name="asyncCorrelationManager">The async correlation manager.</param>
    /// <param name="correlatedTask">The task to execute.</param>
    /// <returns>An awaitable that completes once the <paramref name="correlatedTask" /> has executed and the correlation context has disposed.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
    /// </remarks>
    public static Task CorrelateAsync(this IAsyncCorrelationManager asyncCorrelationManager, Func<Task> correlatedTask)
    {
        return asyncCorrelationManager.CorrelateAsync(null, correlatedTask);
    }

    /// <summary>
    /// Executes the <paramref name="correlatedTask" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <param name="asyncCorrelationManager">The async correlation manager.</param>
    /// <param name="correlatedTask">The task to execute.</param>
    /// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
    /// <returns>An awaitable that completes once the <paramref name="correlatedTask" /> has executed and the correlation context has disposed.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
    /// </remarks>
    public static Task CorrelateAsync(this IAsyncCorrelationManager asyncCorrelationManager, Func<Task> correlatedTask, OnException? onException)
    {
        if (asyncCorrelationManager is null)
        {
            throw new ArgumentNullException(nameof(asyncCorrelationManager));
        }

        return asyncCorrelationManager.CorrelateAsync(null, correlatedTask, onException);
    }

    /// <summary>
    /// Executes the <paramref name="correlatedTask" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <param name="asyncCorrelationManager">The async correlation manager.</param>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <param name="correlatedTask">The task to execute.</param>
    /// <returns>An awaitable that completes once the <paramref name="correlatedTask" /> has executed and the correlation context has disposed.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
    /// </remarks>
    public static Task CorrelateAsync(this IAsyncCorrelationManager asyncCorrelationManager, string? correlationId, Func<Task> correlatedTask)
    {
        if (asyncCorrelationManager is null)
        {
            throw new ArgumentNullException(nameof(asyncCorrelationManager));
        }

        return asyncCorrelationManager.CorrelateAsync(correlationId, correlatedTask, null);
    }

    /// <summary>
    /// Executes the <paramref name="correlatedTask" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <typeparam name="T">The return type of the awaitable task.</typeparam>
    /// <param name="asyncCorrelationManager">The async correlation manager.</param>
    /// <param name="correlatedTask">The task to execute.</param>
    /// <returns>An awaitable that completes with a result <typeparamref name="T" /> once the <paramref name="correlatedTask" /> has executed and the correlation context has disposed.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
    /// </remarks>
    public static Task<T> CorrelateAsync<T>(this IAsyncCorrelationManager asyncCorrelationManager, Func<Task<T>> correlatedTask)
    {
        return asyncCorrelationManager.CorrelateAsync(null, correlatedTask);
    }

    /// <summary>
    /// Executes the <paramref name="correlatedTask" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <typeparam name="T">The return type of the awaitable task.</typeparam>
    /// <param name="asyncCorrelationManager">The async correlation manager.</param>
    /// <param name="correlatedTask">The task to execute.</param>
    /// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
    /// <returns>An awaitable that completes with a result <typeparamref name="T" />  once the <paramref name="correlatedTask" /> has executed and the correlation context has disposed.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
    /// </remarks>
    public static Task<T> CorrelateAsync<T>(this IAsyncCorrelationManager asyncCorrelationManager, Func<Task<T>> correlatedTask, OnException<T>? onException)
    {
        if (asyncCorrelationManager is null)
        {
            throw new ArgumentNullException(nameof(asyncCorrelationManager));
        }

        return asyncCorrelationManager.CorrelateAsync(null, correlatedTask, onException);
    }

    /// <summary>
    /// Executes the <paramref name="correlatedTask" /> with its own <see cref="CorrelationContext" />.
    /// </summary>
    /// <typeparam name="T">The return type of the awaitable task.</typeparam>
    /// <param name="asyncCorrelationManager">The async correlation manager.</param>
    /// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
    /// <param name="correlatedTask">The task to execute.</param>
    /// <returns>An awaitable that completes with a result <typeparamref name="T" /> once the <paramref name="correlatedTask" /> has executed and the correlation context has disposed.</returns>
    /// <remarks>
    /// When logging and tracing are both disabled, no correlation context is created and the task simply executed as it normally would.
    /// </remarks>
    public static Task<T> CorrelateAsync<T>(this IAsyncCorrelationManager asyncCorrelationManager, string? correlationId, Func<Task<T>> correlatedTask)
    {
        if (asyncCorrelationManager is null)
        {
            throw new ArgumentNullException(nameof(asyncCorrelationManager));
        }

        return asyncCorrelationManager.CorrelateAsync(correlationId, correlatedTask, null);
    }
}
