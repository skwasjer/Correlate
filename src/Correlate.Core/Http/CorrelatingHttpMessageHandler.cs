using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Correlate.Http
{
	/// <summary>
	/// Enriches the outgoing request with a correlation id header provided by <see cref="ICorrelationContextAccessor"/>.
	/// </summary>
	public class CorrelatingHttpMessageHandler : DelegatingHandler
	{
		private readonly ICorrelationContextAccessor _correlationContextAccessor;
		private readonly CorrelateClientOptions _options;

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelatingHttpMessageHandler"/> class using specified context accessor.
		/// </summary>
		/// <param name="correlationContextAccessor">The correlation context accessor.</param>
		/// <param name="options">The client correlation options.</param>
		public CorrelatingHttpMessageHandler(ICorrelationContextAccessor correlationContextAccessor, IOptions<CorrelateClientOptions> options)
			: base()
		{
			_correlationContextAccessor = correlationContextAccessor;
			_options = options?.Value ?? new CorrelateClientOptions();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CorrelatingHttpMessageHandler"/> class using specified context accessor.
		/// </summary>
		/// <param name="correlationContextAccessor">The correlation context accessor.</param>
		/// <param name="options">The client correlation options.</param>
		/// <param name="innerHandler">The inner handler.</param>
		public CorrelatingHttpMessageHandler(ICorrelationContextAccessor correlationContextAccessor, IOptions<CorrelateClientOptions> options, HttpMessageHandler innerHandler)
			: base(innerHandler)
		{
			_correlationContextAccessor = correlationContextAccessor;
			_options = options?.Value ?? new CorrelateClientOptions();
		}

		/// <inheritdoc />
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			string correlationId = _correlationContextAccessor?.CorrelationContext?.CorrelationId;
			if (correlationId != null)
			{
				if (!request.Headers.Contains(_options.RequestHeader))
				{
					request.Headers.TryAddWithoutValidation(_options.RequestHeader, correlationId);
				}
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}
