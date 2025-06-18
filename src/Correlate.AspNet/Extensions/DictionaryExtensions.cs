using System;
using System.Collections;

namespace Correlate.WebApiTestNet48.Extensions;

internal static class DictionaryExtensions
{
    public static bool TryGetValue<TKey, TValue>(this IDictionary dictionary, TKey key, out TValue? value) where TKey : notnull
    {
        if (dictionary is null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }
        
        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (dictionary.Contains(key))
        {
            value = (TValue)dictionary[key];
            return true;
        }
        
        value = default;
        return false;
    }
}
