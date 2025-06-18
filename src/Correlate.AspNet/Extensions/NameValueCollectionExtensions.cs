using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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
        return nameValueCollection.AllKeys
            .ToDictionary(key2 => key2, _ => nameValueCollection[key])
            .TryAdd(key, value);

    }

    private static bool TryGetValue(this NameValueCollection nameValueCollection, string key, out StringValues value)
    {
        return nameValueCollection.AllKeys
            .ToDictionary(key2 => key2, _ => (StringValues)nameValueCollection.GetValues(key))
            .TryGetValue(key, out value);
    }
}
