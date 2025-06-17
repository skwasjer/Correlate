using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Correlate.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace Correlate.WebApiTestNet48.Extensions;

public static class NameValueCollectionExtensions
{
    
    public static KeyValuePair<string, string?> GetCorrelationIdHeader(this NameValueCollection httpHeaders, IReadOnlyCollection<string> acceptedHeaders)
    {
        if (httpHeaders is null)
        {
            throw new ArgumentNullException(nameof(httpHeaders));
        }

        var typedDictionary = new Dictionary<string, StringValues>();
        foreach (string key in httpHeaders.AllKeys)
        {
            if (key is null)
            {
                continue;
            }

            if (acceptedHeaders.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                typedDictionary[key] = new StringValues(httpHeaders.GetValues(key));
            }
        }

        return typedDictionary.GetCorrelationIdHeader(acceptedHeaders);
    }

    public static bool TryAdd(this NameValueCollection httpHeaders, string key, string value)
    {
        if (httpHeaders is null)
        {
            throw new ArgumentNullException(nameof(httpHeaders));
        }

        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));
        }
        
        if (httpHeaders.GetValues(key) == null)
        {
            httpHeaders.Add(key, value);
            return true;
        }

        return false;
    }
}
