using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Correlate.Http.Extensions
{
	internal static class HttpRequestExtensions
	{
		public static KeyValuePair<string, string> GetCorrelationIdHeader(this HttpRequest httpRequest)
		{
			return GetCorrelationIdHeader(httpRequest, CorrelationHttpHeaders.CorrelationId);
		}

		public static KeyValuePair<string, string> GetCorrelationIdHeader(this HttpRequest httpRequest, params string[] acceptedHeaders)
		{
			string correlationId = null;
			string headerName = null;

			if (acceptedHeaders != null)
			{
				foreach (string requestHeaderName in acceptedHeaders)
				{
					if (httpRequest.Headers.TryGetValue(requestHeaderName, out StringValues value))
					{
						headerName = requestHeaderName;
						correlationId = value.FirstOrDefault();
						if (!string.IsNullOrWhiteSpace(correlationId))
						{
							break;
						}
					}
				}
			}

			return new KeyValuePair<string, string>(
				headerName ?? acceptedHeaders?.FirstOrDefault() ?? CorrelationHttpHeaders.CorrelationId,
				correlationId
			);
		}
	}
}
