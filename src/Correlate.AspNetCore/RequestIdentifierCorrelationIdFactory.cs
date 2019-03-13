using Correlate.Abstractions;
using Microsoft.AspNetCore.Http.Features;

namespace Correlate.AspNetCore
{
	/// <summary>
	/// Produces a base32 encoded correlation id, similar to the <see cref="IHttpRequestIdentifierFeature.TraceIdentifier"/>
	/// </summary>
	public class RequestIdentifierCorrelationIdFactory : ICorrelationIdFactory
	{
		private readonly IHttpRequestIdentifierFeature _httpRequestIdentifierFeature;

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
			return _httpRequestIdentifierFeature.TraceIdentifier;
		}
	}
}
