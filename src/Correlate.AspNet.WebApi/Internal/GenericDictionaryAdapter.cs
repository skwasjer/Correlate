using System.Collections;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Correlate.Internal;

internal sealed class GenericDictionaryAdapter(IDictionary items)
    : IDictionary<object, object?>
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    private readonly IDictionary _items = items ?? throw new ArgumentNullException(nameof(items));

    public IEnumerator<KeyValuePair<object, object?>> GetEnumerator()
    {
        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (DictionaryEntry e in _items)
        {
            yield return new KeyValuePair<object, object?>(e.Key, e.Value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<object, object?> item)
    {
        _items.Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public bool Contains(KeyValuePair<object, object?> item)
    {
        return TryGetValue(item.Key, out object? v) && EqualityComparer<object?>.Default.Equals(v, item.Value);
    }

    public void CopyTo(KeyValuePair<object, object?>[] array, int arrayIndex)
    {
        foreach (KeyValuePair<object, object?> kvp in this)
        {
            array[arrayIndex++] = kvp;
        }
    }

    public bool Remove(KeyValuePair<object, object?> item)
    {
        return Contains(item) && Remove(item.Key);
    }

    public int Count { get => _items.Count; }

    bool ICollection<KeyValuePair<object, object?>>.IsReadOnly { get => _items.IsReadOnly; }

    public bool ContainsKey(object key)
    {
        return _items.Contains(key);
    }

    public void Add(object key, object? value)
    {
        _items.Add(key, value);
    }

    public bool Remove(object key)
    {
        bool exists = ContainsKey(key);
        _items.Remove(key);
        return exists;
    }

    public bool TryGetValue(object key, out object? value)
    {
        if (ContainsKey(key))
        {
            value = _items[key];
            return true;
        }

        value = null;
        return false;
    }

    public object? this[object key]
    {
        get => ContainsKey(key)
            ? _items[key]
            : throw new KeyNotFoundException($"Key {key} not found.");
        set => _items[key] = value;
    }

    public ICollection<object> Keys { get => new ReadOnlyCollection<object>(_items.Keys); }

    public ICollection<object?> Values { get => new ReadOnlyCollection<object?>(_items.Values); }

    private sealed class ReadOnlyCollection<T>(ICollection items) : ICollection<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            return items.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [ExcludeFromCodeCoverage]
        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        [ExcludeFromCodeCoverage]
        public void Clear()
        {
            throw new NotSupportedException();
        }

        [ExcludeFromCodeCoverage]
        public bool Contains(T item)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        [ExcludeFromCodeCoverage]
        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public int Count => items.Count;
        public bool IsReadOnly => true;
    }
}
