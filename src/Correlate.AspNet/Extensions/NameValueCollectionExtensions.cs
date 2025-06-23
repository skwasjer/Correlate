using System.Collections.Specialized;
using Correlate.Http;
using Microsoft.Extensions.Primitives;

namespace Correlate.AspNet.Extensions;

internal static class NameValueCollectionExtensions
{
    internal static KeyValuePair<string, string?> GetCorrelationIdHeader(this NameValueCollection httpHeaders, IReadOnlyCollection<string> acceptedHeaders)
    {
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

    internal static bool TryAdd(this NameValueCollection nameValueCollection, string key, string value)
    {
        if (nameValueCollection == null)
        {
            throw new ArgumentNullException(nameof(nameValueCollection));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        bool keyExists = nameValueCollection
            .AllKeys
            .Any(x => string.Equals(x, key, StringComparison.OrdinalIgnoreCase));

        if (keyExists)
        {
            return false;
        }

        nameValueCollection.Add(key, value);
        return true;
    }

    private static bool TryGetValue(this NameValueCollection nameValueCollection, string key, out StringValues value)
    {
        if (nameValueCollection == null)
        {
            throw new ArgumentNullException(nameof(nameValueCollection));
        }

        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        string? foundKey = nameValueCollection
            .AllKeys
            .FirstOrDefault(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));

        if (foundKey != null)
        {
            string[] values = nameValueCollection.GetValues(foundKey)!;
            value = new StringValues(values);
            return true;
        }

        value = StringValues.Empty;
        return false;
    }
}
