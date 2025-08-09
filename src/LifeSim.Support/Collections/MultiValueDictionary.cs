using System.Collections.Generic;

namespace LifeSim.Support.Collections;

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

    public void Add(TKey key, TValue value)
    {
        if (!this._data.TryGetValue(key, out var list))
        {
            list = new SwapPopList<TValue>(capacity: 1);
            this._data[key] = list;
        }

        list.Add(value);
    }

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

    public IReadOnlyList<TValue> this[TKey key]
    {
        get
        {
            if (!this._data.TryGetValue(key, out var list)) return this._emptyList;
            return list;
        }
    }
}
