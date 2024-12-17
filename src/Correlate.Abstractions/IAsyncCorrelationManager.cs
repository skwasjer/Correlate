﻿namespace Correlate;

/// <summary>
/// Describes methods for starting a correlation context asynchronously.
/// </summary>
public interface IAsyncCorrelationManager
{
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
    public Task CorrelateAsync(string? correlationId, Func<Task> correlatedTask, OnError? onError);

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
    public Task<T> CorrelateAsync<T>(string? correlationId, Func<Task<T>> correlatedTask, OnError<T>? onError);
}
