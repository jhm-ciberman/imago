using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LifeSim.Support.Collections;

/// <summary>
/// Represents a list that inserts and removes in O(1) by assuming the order of elements is not important.
/// By comparison, the default implementation of System.Collections.Generic.List`T` removes in O(n).
/// This is made by swapping the last element with the element to be removed.
/// </summary>
/// <remarks>
/// This class asumes that the order of the elements is not important. Also it asumes the IEnumerable interface is not used concurrently.
/// </remarks>
/// <typeparam name="T">The type of the elements in the list.</typeparam>
public class SwapPopList<T> : IList<T>, ICollection<T>, IReadOnlyList<T>, IEnumerable<T>
{
    private readonly List<T> _list;

    public SwapPopList(int capacity)
    {
        this._list = new List<T>(capacity);
    }

    public SwapPopList()
    {
        this._list = new List<T>();
    }

    public SwapPopList(IEnumerable<T> collection)
    {
        this._list = new List<T>(collection);
    }

    public int Count => this._list.Count;

    public bool IsReadOnly => ((ICollection<T>)this._list).IsReadOnly;

    public T this[int index] { get => this._list[index]; set => this._list[index] = value; }

    public int IndexOf(T item)
    {
        return this._list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        throw new NotSupportedException("Insert method is not supported since SwapPopList does not guarantee any order of the elements");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        int last = this._list.Count - 1;
        this._list[index] = this._list[last];
        this._list.RemoveAt(last);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        this._list.Add(item);
    }

    public void Clear()
    {
        this._list.Clear();
    }

    public bool Contains(T item)
    {
        return this._list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        this._list.CopyTo(array, arrayIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        int index = this._list.LastIndexOf(item);
        if (index >= 0)
        {
            this.RemoveAt(index);
            return true;
        }
        return false;
    }

    public void Sort(IComparer<T> comparer)
    {
        this._list.Sort(comparer);
    }

    public void Sort()
    {
        this._list.Sort();
    }

    public void Sort(Comparison<T> comparison)
    {
        this._list.Sort(comparison);
    }

    public List<T>.Enumerator GetEnumerator()
    {
        // This is a struct, so it's not boxed
        return this._list.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return this.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<T>)this).GetEnumerator();
    }
}
