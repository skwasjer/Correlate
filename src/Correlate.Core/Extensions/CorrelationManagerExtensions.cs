using System;

// ReSharper disable once CheckNamespace - Justification: common extension methods for the manager should be readily accessible.
namespace Correlate
{
	/// <summary>
	/// Extensions for <see cref="ICorrelationManager"/>.
	/// </summary>
	public static class CorrelationManagerExtensions
	{
		/// <summary>
		/// Executes the <paramref name="correlatedAction"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlatedAction">The action to execute.</param>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public static void Correlate(this ICorrelationManager correlationManager, Action correlatedAction)
		{
			correlationManager.Correlate(correlatedAction, null);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedAction"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlatedAction">The action to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public static void Correlate(this ICorrelationManager correlationManager, Action correlatedAction, OnException? onException)
		{
			if (correlationManager is null)
			{
				throw new ArgumentNullException(nameof(correlationManager));
			}

			correlationManager.Correlate(null, correlatedAction, onException);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedAction"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedAction">The action to execute.</param>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public static void Correlate(this ICorrelationManager correlationManager, string? correlationId, Action correlatedAction)
		{
			if (correlationManager is null)
			{
				throw new ArgumentNullException(nameof(correlationManager));
			}

			correlationManager.Correlate(correlationId, correlatedAction, null);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedFunc"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <typeparam name="T">The return type.</typeparam>
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlatedFunc">The func to execute.</param>
		/// <returns>Returns the result of the <paramref name="correlatedFunc"/>.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public static T Correlate<T>(this ICorrelationManager correlationManager, Func<T> correlatedFunc)
		{
			return correlationManager.Correlate(correlatedFunc, null);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedFunc"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <typeparam name="T">The return type.</typeparam>
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlatedFunc">The func to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <returns>Returns the result of the <paramref name="correlatedFunc"/>.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public static T Correlate<T>(this ICorrelationManager correlationManager, Func<T> correlatedFunc, OnException<T>? onException)
		{
			if (correlationManager is null)
			{
				throw new ArgumentNullException(nameof(correlationManager));
			}

			return correlationManager.Correlate(null, correlatedFunc, onException);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedFunc"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <typeparam name="T">The return type.</typeparam>
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedFunc">The func to execute.</param>
		/// <returns>Returns the result of the <paramref name="correlatedFunc"/>.</returns>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public static T Correlate<T>(this ICorrelationManager correlationManager, string? correlationId, Func<T> correlatedFunc)
		{
			if (correlationManager is null)
			{
				throw new ArgumentNullException(nameof(correlationManager));
			}

			return correlationManager.Correlate(correlationId, correlatedFunc, null);
		}

	}
}
