using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Correlate.Abstractions;

namespace Correlate.Http
{
	/// <summary>
	/// Enriches the outgoing request with a correlation id header provided by <see cref="ICorrelationContextAccessor"/>.
	/// </summary>
	public class CorrelatingHttpMessageHandler : DelegatingHandler
	{
		private readonly ICorrelationContextAccessor _correlationContextAccessor;

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationContextFactory"/> class using specified context accessor.
		/// </summary>
		/// <param name="correlationContextAccessor">The correlation context accessor.</param>
		public CorrelatingHttpMessageHandler(ICorrelationContextAccessor correlationContextAccessor)
			: base()
		{
			_correlationContextAccessor = correlationContextAccessor;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelationContextFactory"/> class using specified context accessor.
		/// </summary>
		/// <param name="correlationContextAccessor">The correlation context accessor.</param>
		/// <param name="innerHandler">The inner handler.</param>
		public CorrelatingHttpMessageHandler(ICorrelationContextAccessor correlationContextAccessor, HttpMessageHandler innerHandler)
			: base(innerHandler)
		{
			_correlationContextAccessor = correlationContextAccessor;
		}

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string correlationId = _correlationContextAccessor?.CorrelationContext?.CorrelationId;
			if (correlationId != null)
			{
				if (!request.Headers.Contains(CorrelationHttpHeaders.CorrelationId))
				{
					request.Headers.TryAddWithoutValidation(CorrelationHttpHeaders.CorrelationId, correlationId);
				}
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}
