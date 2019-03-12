using Correlate.Abstractions;
using System;

namespace Correlate
{
	public class CorrelationContextFactory : ICorrelationContextFactory
	{
		private readonly ICorrelationContextAccessor _correlationContextAccessor;

		public CorrelationContextFactory()
		{
		}

		public CorrelationContextFactory(ICorrelationContextAccessor correlationContextAccessor)
			: this()
		{
			_correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
		}

		CorrelationContext ICorrelationContextFactory.Create(string correlationId)
		{
			CorrelationContext correlationContext = Create(correlationId);
			if (_correlationContextAccessor != null)
			{
				_correlationContextAccessor.CorrelationContext = correlationContext;
			}

			return correlationContext;
		}

		public virtual CorrelationContext Create(string correlationId)
		{
			return new CorrelationContext
			{
				CorrelationId = correlationId
			};
		}

		public void Dispose()
		{
			if (_correlationContextAccessor != null)
			{
				_correlationContextAccessor.CorrelationContext = null;
			}
		}
	}
}
