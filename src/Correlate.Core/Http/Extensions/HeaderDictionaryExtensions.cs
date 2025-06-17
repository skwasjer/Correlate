using Microsoft.Extensions.Primitives;

namespace Correlate.Http.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IDictionary{TKey, TValue}"/> to retrieve correlation ID headers.
/// </summary>
public static class HeaderDictionaryExtensions
{
    /// <summary>
    /// Gets the correlation ID header from the provided HTTP headers.
    /// </summary>
    /// <param name="httpHeaders"></param>
    /// <param name="acceptedHeaders"></param>
    /// <returns>A key value pair with correlation id and its header name</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static KeyValuePair<string, string?> GetCorrelationIdHeader(this IDictionary<string, StringValues> httpHeaders, IReadOnlyCollection<string> acceptedHeaders)
    {
        if (httpHeaders is null)
        {
            throw new ArgumentNullException(nameof(httpHeaders));
        }
        
        if (acceptedHeaders is null)
        {
            throw new ArgumentNullException(nameof(acceptedHeaders));
        }

        if (acceptedHeaders.Count == 0)
        {
            return new KeyValuePair<string, string?>(CorrelationHttpHeaders.CorrelationId, null);
        }

        string? correlationId = null;
        string? headerName = null;

        foreach (string requestHeaderName in acceptedHeaders)
        {
            if (!httpHeaders.TryGetValue(requestHeaderName, out StringValues value))
            {
                continue;
            }

            headerName = requestHeaderName;
            correlationId = value.LastOrDefault();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                break;
            }
        }

        return new KeyValuePair<string, string?>(
            headerName ?? acceptedHeaders.First(),
            correlationId
        );
    }
}
