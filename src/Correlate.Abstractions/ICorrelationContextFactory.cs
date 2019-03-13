namespace Correlate.Abstractions
{
	/// <summary>
	/// Describes how to create/clean up a <see cref="CorrelationContext"/>.
	/// </summary>
	public interface ICorrelationContextFactory
	{
		/// <summary>
		/// Creates a new <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationId">The correlation id to associate to the context.</param>
		/// <returns>The <see cref="CorrelationContext"/> containing the correlation id.</returns>
		CorrelationContext Create(string correlationId);

		/// <summary>
		/// Disposes the <see cref="CorrelationContext"/>.
		/// </summary>
		void Dispose();
	}
}
