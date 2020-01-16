using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace Correlate.Http.Extensions
{
	internal static class HeaderDictionaryExtensions
	{
		internal static KeyValuePair<string, string?> GetCorrelationIdHeader(this IDictionary<string, StringValues> httpHeaders, ICollection<string> acceptedHeaders)
		{
			if (acceptedHeaders == null)
			{
				throw new ArgumentNullException(nameof(acceptedHeaders));
			}

			if (!acceptedHeaders.Any())
			{
				return new KeyValuePair<string, string?>(CorrelationHttpHeaders.CorrelationId, null);
			}

			string? correlationId = null;
			string? headerName = null;

			foreach (string requestHeaderName in acceptedHeaders)
			{
				if (httpHeaders.TryGetValue(requestHeaderName, out StringValues value))
				{
					headerName = requestHeaderName;
					correlationId = value.LastOrDefault();
					if (!string.IsNullOrWhiteSpace(correlationId))
					{
						break;
					}
				}
			}

			return new KeyValuePair<string, string?>(
				headerName ?? acceptedHeaders.First(),
				correlationId
			);
		}
	}
}
