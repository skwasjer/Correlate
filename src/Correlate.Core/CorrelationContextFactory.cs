using System;

namespace Correlate
{
	/// <summary>
	/// Factory to create/clean up a <see cref="CorrelationContext"/> and optionally associate it with a <see cref="ICorrelationContextAccessor"/>.
	/// </summary>
	public class CorrelationContextFactory : ICorrelationContextFactory
	{
		private readonly ICorrelationContextAccessor? _correlationContextAccessor;

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationContextFactory"/> class.
		/// </summary>
		public CorrelationContextFactory()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationContextFactory"/> class using specified context accessor.
		/// </summary>
		/// <param name="correlationContextAccessor">The correlation context accessor.</param>
		public CorrelationContextFactory(ICorrelationContextAccessor correlationContextAccessor)
			: this()
		{
			_correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
		}

		/// <inheritdoc />
		CorrelationContext ICorrelationContextFactory.Create(string correlationId)
		{
			CorrelationContext correlationContext = Create(correlationId);
			if (_correlationContextAccessor != null)
			{
				_correlationContextAccessor.CorrelationContext = correlationContext;
			}

			return correlationContext;
		}

		/// <summary>
		/// Creates a new <see cref="CorrelationContext"/>.
		/// </summary>
		/// <param name="correlationId">The correlation id to associate to the context.</param>
		/// <returns>The <see cref="CorrelationContext"/> containing the correlation id.</returns>
		public virtual CorrelationContext Create(string correlationId)
		{
			return new CorrelationContext
			{
				CorrelationId = correlationId
			};
		}

		/// <inheritdoc />
		public void Dispose()
		{
			if (_correlationContextAccessor != null)
			{
				_correlationContextAccessor.CorrelationContext = null;
			}
		}
	}
}
