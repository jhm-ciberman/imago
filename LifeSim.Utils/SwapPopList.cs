using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LifeSim
{
    // List that inserts and removes in O(1). (Regular System.Collections.Generic.List<T> removes in O(n))
    // This list does not conserves the order of the elements when removing an element.
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
            throw new System.NotSupportedException("Insert method is not supported since SwapPopList does not guarantee any order of the elements");
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

        public IEnumerator<T> GetEnumerator()
        {
            return this._list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._list.GetEnumerator();
        }
    }
}