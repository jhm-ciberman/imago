using System.Collections.Generic;

namespace Imago.Support.Collections;

/// <summary>
/// A dictionary that allows multiple values for a single key. Order of values are not preserved.
/// The dictionary always returns a list of values for a key, even if the list is empty.
/// </summary>
/// <typeparam name="TKey">The type of the keys in the dictionary. Must be a non-nullable type.</typeparam>
/// <typeparam name="TValue">The type of the values in the dictionary. Must be a non-nullable type.</typeparam>
public class MultiValueDictionary<TKey, TValue> where TKey : notnull
{
    private readonly Dictionary<TKey, SwapPopList<TValue>> _data = new();

    private readonly List<TValue> _emptyList = new();

    /// <summary>
    /// Adds a value to the collection of values associated with the specified key.
    /// </summary>
    /// <param name="key">The key to associate the value with.</param>
    /// <param name="value">The value to add to the collection.</param>
    public void Add(TKey key, TValue value)
    {
        if (!this._data.TryGetValue(key, out var list))
        {
            list = new SwapPopList<TValue>(capacity: 1);
            this._data[key] = list;
        }

        list.Add(value);
    }

    /// <summary>
    /// Removes a specific value from the collection associated with the specified key.
    /// </summary>
    /// <param name="key">The key to remove the value from.</param>
    /// <param name="value">The value to remove from the collection.</param>
    /// <returns>true if the value was found and removed; otherwise, false.</returns>
    public bool Remove(TKey key, TValue value)
    {
        if (!this._data.TryGetValue(key, out var list)) return false;
        if (!list.Remove(value)) return false;

        if (list.Count == 0)
        {
            this._data.Remove(key);
        }

        return true;
    }

    /// <summary>
    /// Gets the collection of values associated with the specified key.
    /// </summary>
    /// <param name="key">The key to get the values for.</param>
    /// <returns>A read-only list of values associated with the key, or an empty list if the key is not found.</returns>
    public IReadOnlyList<TValue> this[TKey key]
    {
        get
        {
            if (!this._data.TryGetValue(key, out var list)) return this._emptyList;
            return list;
        }
    }
}
