namespace Correlate.Abstractions
{
	/// <summary>
	/// Provides access to the <see cref="CorrelationContext"/>.
	/// </summary>
	public interface ICorrelationContextAccessor
	{
		/// <summary>
		/// Gets or sets <see cref="CorrelationContext"/>.
		/// </summary>
		CorrelationContext CorrelationContext { get; set; }
	}
}
