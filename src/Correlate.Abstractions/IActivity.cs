namespace Correlate
{
	internal interface IActivity
	{
		void Start(CorrelationContext correlationContext);
		void Stop();
	}
}