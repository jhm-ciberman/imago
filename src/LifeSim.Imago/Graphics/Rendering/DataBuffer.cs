using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Imago.Graphics.Rendering;

/// <summary>
/// Represents a buffer of data that can be uploaded to the GPU and used in shaders.
/// </summary>
internal class DataBuffer : IDisposable
{
    private static int _count = 0;
    private readonly nint _data;
    private readonly int[] _freeList;
    private int _freeListCount = 0;
    private readonly GraphicsDevice _gd;
    private readonly ResourceLayout _resourceLayout;
    private bool _dirty = true;

    /// <summary>
    /// Gets the number of blocks in this buffer.
    /// </summary>
    public int BlocksCount { get; private set; }

    /// <summary>
    /// Gets the size of each block in this buffer.
    /// </summary>
    public int BlockSize { get; private set; }

    /// <summary>
    /// Gets the total size of this buffer in bytes.
    /// </summary>
    public int SizeInBytes { get; private set; }

    /// <summary>
    /// Gets the resource set that can be used to bind this buffer to a shader.
    /// </summary>
    public ResourceSet ResourceSet { get; private set; }

    /// <summary>
    /// Gets the device buffer that is used to store the data.
    /// </summary>
    public DeviceBuffer DeviceBuffer { get; private set; }

    /// <summary>
    /// Gets or sets the name of this buffer.
    /// </summary>
    public string Name { get => this.DeviceBuffer.Name; set => this.DeviceBuffer.Name = value; }

    /// <summary>
    /// Gets a value indicating whether this buffer is full.
    /// </summary>
    public bool IsFull => this._freeListCount == 0;

    /// <summary>
    /// Gets the ID of this buffer.
    /// </summary>
    public int Id { get; private set; }

    public unsafe DataBuffer(GraphicsDevice gd, int blocksCount, int blockSize, ResourceLayout resourceLayout)
    {
        this.Id = ++_count;
        this._gd = gd;
        this.BlockSize = blockSize;
        this.BlocksCount = blocksCount;
        this.SizeInBytes = this.BlocksCount * this.BlockSize;
        this._data = Marshal.AllocHGlobal(this.SizeInBytes);
        fixed (void* dataPtr = &this._data)
        {
            Unsafe.InitBlockUnaligned((byte*)this._data, 0, (uint)this.SizeInBytes);
        }
        this._dirty = true;
        this._resourceLayout = resourceLayout;

        this.DeviceBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(
            (uint)this.SizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic
        ));

        this.ResourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
            this._resourceLayout, this.DeviceBuffer
        ));

        this._freeList = new int[blocksCount];
        for (int i = 0; i < blocksCount; i++)
        {
            this._freeList[i] = (this.BlocksCount - i - 1) * this.BlockSize;
        }
        this._freeListCount = this.BlocksCount;
    }

    /// <summary>
    /// Uploads the data to the GPU.
    /// </summary>
    /// <param name="commandList">The command list to use.</param>
    public void UploadToGPU(CommandList commandList)
    {
        if (!this._dirty) return;
        commandList.UpdateBuffer(this.DeviceBuffer, 0, this._data, (uint)this.SizeInBytes);
        this._dirty = false;
    }

    /// <summary>
    /// Requests a block of data from this buffer.
    /// </summary>
    /// <returns>The requested block.</returns>
    /// <exception cref="InvalidOperationException">Thrown when there are no free blocks left in this buffer.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DataBlock RequestBlock()
    {
        if (this._freeListCount == 0)
            throw new InvalidOperationException($"No free blocks left in buffer {this.Name}. This should never happen.");

        this._freeListCount--;
        return new DataBlock(this, this._freeList[this._freeListCount]);
    }

    /// <summary>
    /// Frees a block of data in this buffer.
    /// </summary>
    /// <param name="offset">The offset of the block to free.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FreeBlock(int offset)
    {
        this._freeList[this._freeListCount] = offset;
        this._freeListCount++;
    }

    /// <summary>
    /// Writes a value to the buffer.
    /// </summary>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    /// <param name="offset">The offset to write to.</param>
    /// <param name="data">The value to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write<T>(int offset, ref T data) where T : unmanaged
    {
        Marshal.StructureToPtr(data, this._data + offset, false);
        this._dirty = true;
    }

    /// <summary>
    /// Writes a value to the buffer.
    /// </summary>
    /// <typeparam name="T">The type of the value to write.</typeparam>
    /// <param name="offset">The offset to write to.</param>
    /// <param name="data">The value to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe void WriteSpan<T>(int offset, ReadOnlySpan<T> data) where T : unmanaged
    {
        fixed (T* ptr = data)
        {
            var byteLen = (long)(data.Length * sizeof(T));
            Buffer.MemoryCopy(ptr, (void*)(this._data + offset), byteLen, byteLen);
        }
        this._dirty = true;
    }

    /// <summary>
    /// Reads a value from the buffer.
    /// </summary>
    /// <typeparam name="T">The type of the value to read.</typeparam>
    /// <param name="offset">The offset to read from.</param>
    /// <returns>The value read.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Read<T>(int offset) where T : struct
    {
        return Marshal.PtrToStructure<T>(this._data + offset);
    }

    /// <summary>
    /// Dispose this buffer.
    /// </summary>
    public void Dispose()
    {
        Marshal.FreeHGlobal(this._data);
    }
}
