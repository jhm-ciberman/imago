using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Imago.Utilities;

/// <summary>
/// Provides unsafe enumeration over data with a custom stride between elements.
/// </summary>
/// <typeparam name="T">The type of elements to enumerate.</typeparam>
/// <remarks>
/// This struct enables efficient iteration over memory where elements are not contiguous
/// but separated by a fixed stride. Commonly used for vertex buffer data and other
/// interleaved memory layouts.
/// </remarks>
public readonly unsafe struct StrideHelper<T>
{
    private readonly byte* _basePtr;
    private readonly int _count;
    private readonly int _stride;

    /// <summary>
    /// Initializes a new instance of the <see cref="StrideHelper{T}"/> struct.
    /// </summary>
    /// <param name="ptr">Pointer to the start of the data.</param>
    /// <param name="count">Number of elements to enumerate.</param>
    /// <param name="stride">Size in bytes between consecutive elements.</param>
    public StrideHelper(void* ptr, int count, int stride)
    {
        this._basePtr = (byte*)ptr;
        this._count = count;
        this._stride = stride;
    }

    /// <summary>
    /// Gets an enumerator that iterates through the strided data.
    /// </summary>
    /// <returns>An enumerator for the strided data.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator(this._basePtr, this._count, this._stride);
    }

    /// <summary>
    /// Provides enumeration over strided data elements.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly byte* _basePtr;
        private readonly int _count;
        private readonly int _stride;
        private int _currentItemIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="Enumerator"/> struct.
        /// </summary>
        /// <param name="basePtr">Pointer to the start of the data.</param>
        /// <param name="count">Number of elements to enumerate.</param>
        /// <param name="stride">Size in bytes between consecutive elements.</param>
        public Enumerator(byte* basePtr, int count, int stride)
        {
            this._basePtr = basePtr;
            this._count = count;
            this._stride = stride;
            this._currentItemIndex = -1;
        }

        /// <summary>
        /// Gets the current element in the enumeration.
        /// </summary>
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

        /// <summary>
        /// Disposes the enumerator.
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Advances the enumerator to the next element.
        /// </summary>
        /// <returns>true if the enumerator successfully advanced to the next element; false if the enumerator has passed the end of the collection.</returns>
        public bool MoveNext()
        {
            this._currentItemIndex += 1;
            return this._currentItemIndex < this._count;
        }

        /// <summary>
        /// Sets the enumerator to its initial position, which is before the first element in the collection.
        /// </summary>
        public void Reset()
        {
            this._currentItemIndex = -1;
        }
    }
}
