using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Correlate.AspNet.Extensions;

internal static class DictionaryExtensions
{
    public static bool TryGetValue<TKey, TValue>(
        this IDictionary dictionary, 
        TKey key, 
        [MaybeNullWhen(false)] out TValue value
    ) where TKey : notnull
    {
        if (dictionary == null)
        {
            throw new ArgumentNullException(nameof(dictionary));
        }

        if (EqualityComparer<TKey>.Default.Equals(key, default))
        {
            throw new ArgumentNullException(nameof(key));
        }

        if (dictionary.Contains(key))
        {
            object? result = dictionary[key];
        
            if (result is TValue typedValue)
            {
                value = typedValue;
                return true;
            }
        }

        value = default!;
        return false;
    }
}
