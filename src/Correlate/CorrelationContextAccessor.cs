using System.Threading;
using Correlate.Abstractions;

namespace Correlate
{
	public class CorrelationContextAccessor : ICorrelationContextAccessor
	{
		private static readonly AsyncLocal<CorrelationContextHolder> CurrentContext = new AsyncLocal<CorrelationContextHolder>();

		public CorrelationContext CorrelationContext
		{
			get => CurrentContext.Value?.Context;
			set
			{
				CorrelationContextHolder holder = CurrentContext.Value;
				if (holder != null)
				{
					// Clear current HttpContext trapped in the AsyncLocals, as its done.
					holder.Context = null;
				}

				if (value != null)
				{
					// Use an object indirection to hold the HttpContext in the AsyncLocal,
					// so it can be cleared in all ExecutionContexts when its cleared.
					CurrentContext.Value = new CorrelationContextHolder
					{
						Context = value
					};
				}
			}
		}

		private class CorrelationContextHolder
		{
			public CorrelationContext Context;
		}
	}
}
