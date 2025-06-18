using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Correlate.AspNet.Extensions;

internal static class DictionaryExtensions
{
    /// <summary>
    /// Adding missing method introduced in .net core<br/>
    /// <see href="https://stackoverflow.com/a/68711860/2890855" />
    /// </summary>
    public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        if (!dictionary.ContainsKey(key))
        {
            dictionary.Add(key, value);
            return true;
        }

        return false;
    }
    
    public static bool TryGetValue<TKey, TValue>(this IDictionary dictionary, TKey key, out TValue? value) where TKey : notnull
    {
        return dictionary.Keys
            .Cast<object>()
            .ToDictionary<object, object, TValue?>(key2 => key2, value => (TValue?)dictionary[key])
            .TryGetValue(key, out value);
    }
}
