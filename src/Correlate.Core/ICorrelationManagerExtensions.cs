using System;

namespace Correlate
{
	/// <summary>
	/// Extensions for <see cref="ICorrelationManager"/>.
	/// </summary>
	public static class ICorrelationManagerExtensions
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
		public static void Correlate(this ICorrelationManager correlationManager, Action correlatedAction, OnException onException)
		{
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
		public static void Correlate(this ICorrelationManager correlationManager, string correlationId, Action correlatedAction)
		{
			correlationManager.Correlate(correlationId, correlatedAction, null);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedFunc"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlatedFunc">The func to execute.</param>
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
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlatedFunc">The func to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public static T Correlate<T>(this ICorrelationManager correlationManager, Func<T> correlatedFunc, OnException onException)
		{
			return correlationManager.Correlate(null, correlatedFunc, onException);
		}

		/// <summary>
		/// Executes the <paramref name="correlatedFunc"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationManager">The correlation manager.</param>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedFunc">The func to execute.</param>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		public static T Correlate<T>(this ICorrelationManager correlationManager, string correlationId, Func<T> correlatedFunc)
		{
			return correlationManager.Correlate(correlationId, correlatedFunc, null);
		}

	}
}
