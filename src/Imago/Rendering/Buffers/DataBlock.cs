using System;
using System.Runtime.CompilerServices;

namespace Imago.Rendering.Buffers;

/// <summary>
/// Represents a block of data in a <see cref="DataBuffer"/>.
/// </summary>
/// <summary>
/// Represents a block of data in a <see cref="DataBuffer"/>.
/// </summary>
internal struct DataBlock : IDisposable
{
    /// <summary>
    /// The <see cref="DataBuffer"/> that this block belongs to.
    /// </summary>
    internal DataBuffer Buffer { get; set; }

    /// <summary>
    /// The offset of this block within the <see cref="DataBuffer"/>.
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    /// Gets a value indicating whether this block is valid.
    /// </summary>
    public bool IsValid => this.Buffer != null;

    /// <summary>
    /// Gets the size of this block.
    /// </summary>
    public int BlockSize => this.Buffer == null ? 0 : this.Buffer.BlockSize;

    /// <summary>
    /// Gets the index of this block within the <see cref="DataBuffer"/>.
    /// </summary>
    public uint BlockIndex => (uint)(this.Offset / this.Buffer.BlockSize);

    /// <summary>
    /// Initializes a new instance of the <see cref="DataBlock"/> struct.
    /// </summary>
    /// <param name="buffer">The <see cref="DataBuffer"/> that this block belongs to.</param>
    /// <param name="offset">The offset of this block within the <see cref="DataBuffer"/>.</param>
    internal DataBlock(DataBuffer buffer, int offset)
    {
        this.Buffer = buffer;
        this.Offset = offset;
    }

    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from this block.
    /// </summary>
    /// <typeparam name="T">The type of the value to read.</typeparam>
    /// <returns>The value that was read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>() where T : unmanaged
    {
        return this.Buffer.Read<T>(this.Offset);
    }

    /// <summary>
    /// Writes a value of type <typeparamref name="T"/> to this block.
    /// </summary>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    /// <param name="data">The value to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T>(ref T data) where T : unmanaged
    {
        this.Buffer.Write(this.Offset, ref data);
    }

    /// <summary>
    /// Writes a span of values of type <typeparamref name="T"/> to this block.
    /// </summary>
    /// <typeparam name="T">The type of the values to write.</typeparam>
    /// <param name="data">The span of values to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSpan<T>(ReadOnlySpan<T> data) where T : unmanaged
    {
        this.Buffer.WriteSpan(this.Offset, data);
    }

    /// <summary>
    /// Writes a span of values of type <typeparamref name="T"/> to this block.
    /// </summary>
    /// <typeparam name="T">The type of the values to write.</typeparam>
    /// <param name="data">The span of values to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteSpan<T>(Span<T> data) where T : unmanaged
    {
        this.Buffer.WriteSpan<T>(this.Offset, data);
    }

    /// <summary>
    /// Frees this block from the <see cref="DataBuffer"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        this.Buffer?.FreeBlock(this.Offset);
        this.Buffer = null!;
    }
}
