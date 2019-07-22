using System;

namespace Correlate
{
	/// <summary>
	/// Describes methods for starting a correlation context asynchronously.
	/// </summary>
	public interface ICorrelationManager
	{
		/// <summary>
		/// Executes the <paramref name="correlatedAction"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedAction">The action to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		void Correlate(string correlationId, Action correlatedAction, OnException onException);

		/// <summary>
		/// Executes the <paramref name="correlatedFunc"/> with its own <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationId">The correlation id to use, or null to generate a new one.</param>
		/// <param name="correlatedFunc">The func to execute.</param>
		/// <param name="onException">A delegate to handle the exception inside the correlation scope, before it is disposed. Returns <see langword="true" /> to consider the exception handled, or <see langword="false" /> to throw.</param>
		/// <remarks>
		/// When logging and tracing are both disabled, no correlation context is created and the action simply executed as it normally would.
		/// </remarks>
		T Correlate<T>(string correlationId, Func<T> correlatedFunc, OnException onException);
	}
}