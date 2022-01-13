using System;
using System.Collections;
using System.Collections.Generic;

namespace LifeSim;

public class PagedArray<T> : ICollection<T> where T : class
{
    protected struct Page
    {
        public T?[] Items;
    }
    protected Page[] _pages;
    protected int _pageCapacity;
    protected int _count;
    protected Queue<int> _freeList;

    public PagedArray(int capacity = 50)
    {
        this._pageCapacity = capacity;
        this._count = 0;
        this._freeList = new Queue<int>(this._pageCapacity);
        this._pages = Array.Empty<PagedArray<T>.Page>();

        this.AllocateNewPage();
    }

    private void AllocateNewPage()
    {
        int oldPagesCount = this._pages.Length;
        int newPagesCount = oldPagesCount + 1;
        Array.Resize(ref this._pages, newPagesCount);
        this._pages[oldPagesCount].Items = new T[this._pageCapacity];

        int oldSize = oldPagesCount * this._pageCapacity;
        int newSize = newPagesCount * this._pageCapacity;
        for (int i = oldSize; i < newSize; i++)
        {
            this._freeList.Enqueue(i);
        }
    }

    public T this[int index]
    {
        get
        {
            if (index >= this._count) throw new IndexOutOfRangeException($"The index {index} is out of range of a maximum of {this._count}");
            int pageIndex = index / this._pageCapacity;
            int pageOffset = index % this._pageCapacity;
            return this._pages[pageIndex].Items[pageOffset] ?? throw new IndexOutOfRangeException();
        }
    }

    public int Count => this._count;
    public int Capacity => this._pageCapacity * this._pages.Length;

    public bool IsReadOnly => false;

    public int Add(T item)
    {
        if (!this._freeList.TryDequeue(out int index))
        {
            this.AllocateNewPage();
            index = this._freeList.Dequeue();
        }

        int pageIndex = index / this._pageCapacity;
        int pageOffset = index % this._pageCapacity;
        this._pages[pageIndex].Items[pageOffset] = item;
        this._count++;
        return index;
    }

    void ICollection<T>.Add(T item)
    {
        this.Add(item);
    }

    public bool Remove(T item)
    {
        int index = this.IndexOf(item);
        if (index < 0) return false;
        this.RemoveAt(index);
        return true;
    }

    public void RemoveAt(int index)
    {
        int pageIndex = index / this._pageCapacity;
        int pageOffset = index % this._pageCapacity;
        this._pages[pageIndex].Items[pageOffset] = default;
        this._freeList.Enqueue(index);
        this._count--;
    }

    public int IndexOf(T item)
    {
        for (int i = 0; i < this._pages.Length; i++)
        {
            var items = this._pages[i].Items;
            for (int j = 0; i < items.Length; j++)
            {
                var currentItem = items[j];
                if (currentItem != null && currentItem.Equals(item))
                {
                    return i * this._pageCapacity + j;
                }
            }
        }
        return -1;
    }


    public void Clear()
    {
        Array.Resize(ref this._pages, 0);
        this._count = 0;
        this._freeList.Clear();
    }

    public bool Contains(T item)
    {
        return this.IndexOf(item) > 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        for (int i = 0; i < this.Count; i++)
        {
            array[arrayIndex + i] = this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    private struct Enumerator : IEnumerator<T>
    {
        private int _index;
        private readonly PagedArray<T> _arr;

        public Enumerator(PagedArray<T> arr)
        {
            this._arr = arr;
            this._index = 0;
        }

        public T Current => this._arr[this._index];

        object? IEnumerator.Current => this.Current;

        public bool MoveNext()
        {
            this._index++;
            return (this._index < this._arr.Count);
        }

        public void Reset()
        {
            this._index = 0;
        }

        public void Dispose()
        {
            //
        }
    }
}