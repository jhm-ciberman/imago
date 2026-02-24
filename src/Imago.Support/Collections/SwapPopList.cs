using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Imago.Support.Collections;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="SwapPopList{T}"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The initial capacity of the list.</param>
    public SwapPopList(int capacity)
    {
        this._list = new List<T>(capacity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwapPopList{T}"/> class.
    /// </summary>
    public SwapPopList()
    {
        this._list = new List<T>();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SwapPopList{T}"/> class with elements from the specified collection.
    /// </summary>
    /// <param name="collection">The collection to copy elements from.</param>
    public SwapPopList(IEnumerable<T> collection)
    {
        this._list = new List<T>(collection);
    }

    /// <summary>
    /// Gets the number of elements in the list.
    /// </summary>
    public int Count => this._list.Count;

    /// <summary>
    /// Gets a value indicating whether the list is read-only.
    /// </summary>
    public bool IsReadOnly => ((ICollection<T>)this._list).IsReadOnly;

    /// <summary>
    /// Gets or sets the element at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <returns>The element at the specified index.</returns>
    public T this[int index] { get => this._list[index]; set => this._list[index] = value; }

    /// <summary>
    /// Determines the index of the first occurrence of a specific item in the list.
    /// </summary>
    /// <param name="item">The item to locate in the list.</param>
    /// <returns>The zero-based index of the first occurrence of item, or -1 if not found.</returns>
    public int IndexOf(T item)
    {
        return this._list.IndexOf(item);
    }

    /// <summary>
    /// This method is not supported because <see cref="SwapPopList{T}"/> does not guarantee element order.
    /// </summary>
    /// <param name="index">The index at which to insert the item.</param>
    /// <param name="item">The item to insert.</param>
    /// <exception cref="NotSupportedException">Always thrown as this operation is not supported.</exception>
    public void Insert(int index, T item)
    {
        throw new NotSupportedException("Insert method is not supported since SwapPopList does not guarantee any order of the elements");
    }

    /// <summary>
    /// Removes the element at the specified index using swap-and-pop for O(1) performance.
    /// </summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        int last = this._list.Count - 1;
        this._list[index] = this._list[last];
        this._list.RemoveAt(last);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        this._list.Add(item);
    }

    /// <summary>
    /// Removes all elements from the list.
    /// </summary>
    public void Clear()
    {
        this._list.Clear();
    }

    /// <summary>
    /// Determines whether the list contains a specific item.
    /// </summary>
    /// <param name="item">The item to locate in the list.</param>
    /// <returns>true if the item is found in the list; otherwise, false.</returns>
    public bool Contains(T item)
    {
        return this._list.Contains(item);
    }

    /// <summary>
    /// Copies the elements of the list to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(T[] array, int arrayIndex)
    {
        this._list.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
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

    /// <summary>
    /// Sorts the elements in the list using the specified comparer.
    /// </summary>
    /// <param name="comparer">The comparer to use when comparing elements.</param>
    public void Sort(IComparer<T> comparer)
    {
        this._list.Sort(comparer);
    }

    /// <summary>
    /// Sorts the elements in the list using the default comparer.
    /// </summary>
    public void Sort()
    {
        this._list.Sort();
    }

    /// <summary>
    /// Sorts the elements in the list using the specified comparison.
    /// </summary>
    /// <param name="comparison">The comparison to use when comparing elements.</param>
    public void Sort(Comparison<T> comparison)
    {
        this._list.Sort(comparison);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the list.
    /// </summary>
    /// <returns>An enumerator for the list.</returns>
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
