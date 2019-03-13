using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Correlate.AspNetCore
{
	// ReSharper disable once InconsistentNaming
	internal static class ILoggerExtensions
	{
		public static IDisposable BeginRequestScope(this ILogger logger, HttpContext httpContext, string correlationId)
		{
			return logger.BeginScope(new RequestLogScope(httpContext, correlationId));
		}

		private class RequestLogScope : IReadOnlyList<KeyValuePair<string, object>>
		{
			private readonly HttpContext _httpContext;
			private readonly string _correlationId;

			public RequestLogScope(HttpContext httpContext, string correlationId)
			{
				_httpContext = httpContext;
				_correlationId = correlationId;
			}

			/// <inheritdoc />
			public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
			{
				yield return this[0];
				yield return this[1];
				yield return this[2];
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			/// <inheritdoc />
			public int Count => 3;

			/// <inheritdoc />
			public KeyValuePair<string, object> this[int index]
			{
				get
				{
					if (index == 0)
					{
						return new KeyValuePair<string, object>("RequestId", _httpContext.TraceIdentifier);
					}

					if (index == 1)
					{
						return new KeyValuePair<string, object>("RequestPath", _httpContext.Request.Path.ToString());
					}

					if (index == 2)
					{
						return new KeyValuePair<string, object>("CorrelationId", _correlationId);
					}

					throw new ArgumentOutOfRangeException(nameof(index));
				}
			}
		}
	}
}
