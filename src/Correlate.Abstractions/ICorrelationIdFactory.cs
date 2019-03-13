namespace Correlate.Abstractions
{
	/// <summary>
	/// Describes a way of generating new correlation ids.
	/// </summary>
	public interface ICorrelationIdFactory
	{
		/// <summary>
		/// Creates a new correlation id.
		/// </summary>
		/// <returns>The new correlation id.</returns>
		string Create();
	}
}
