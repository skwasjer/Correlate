namespace Correlate.Abstractions
{
	public interface ICorrelationContextFactory
	{
		CorrelationContext Create(string correlationId);

		void Dispose();
	}
}
