using Microsoft.AspNetCore.Http.Features;

namespace Correlate
{
	/// <summary>
	/// Produces a base32 encoded correlation id, similar to the <see cref="HttpRequestIdentifierFeature.TraceIdentifier"/>
	/// </summary>
	public class RequestIdentifierCorrelationIdFactory : ICorrelationIdFactory
	{
		private readonly HttpRequestIdentifierFeature _httpRequestIdentifierFeature;

		/// <summary>
		/// Initializes a new instance of the <see cref="RequestIdentifierCorrelationIdFactory"/> class.
		/// </summary>
		public RequestIdentifierCorrelationIdFactory()
		{
			_httpRequestIdentifierFeature = new HttpRequestIdentifierFeature();
		}

		/// <inheritdoc />
		public string Create()
		{
			// Set to null, so the next 'get' will produce a new id.
			_httpRequestIdentifierFeature.TraceIdentifier = null;
			return _httpRequestIdentifierFeature.TraceIdentifier!;
		}
	}
}
