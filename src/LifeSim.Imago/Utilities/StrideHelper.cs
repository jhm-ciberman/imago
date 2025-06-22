using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LifeSim.Imago.Utilities;

public readonly unsafe struct StrideHelper<T>
{
    private readonly byte* _basePtr;
    private readonly int _count;
    private readonly int _stride;

    public StrideHelper(void* ptr, int count, int stride)
    {
        this._basePtr = (byte*)ptr;
        this._count = count;
        this._stride = stride;
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this._basePtr, this._count, this._stride);
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly byte* _basePtr;
        private readonly int _count;
        private readonly int _stride;
        private int _currentItemIndex;

        public Enumerator(byte* basePtr, int count, int stride)
        {
            this._basePtr = basePtr;
            this._count = count;
            this._stride = stride;
            this._currentItemIndex = -1;
        }

        public T Current
        {
            get
            {
                if (this._currentItemIndex == -1 || this._currentItemIndex >= this._count)
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    return Unsafe.Read<T>(this._basePtr + this._currentItemIndex * this._stride);
                }
            }
        }

        object IEnumerator.Current => this.Current!;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            this._currentItemIndex += 1;
            return this._currentItemIndex < this._count;
        }

        public void Reset()
        {
            this._currentItemIndex = -1;
        }
    }
}
