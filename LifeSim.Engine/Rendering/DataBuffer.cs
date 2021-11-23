using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    internal partial class DataBuffer : IDisposable
    {
        private static int _count = 0;
        private readonly IntPtr _data;

        public int BlocksCount { get; private set; }
        public int BlockSize { get; private set; }
        public int SizeInBytes { get; private set; }

        private readonly int[] _freeList;
        private int _freeListCount = 0;
        private readonly GraphicsDevice _gd;
        private readonly ResourceLayout _resourceLayout;

        public ResourceSet ResourceSet { get; private set; }
        public DeviceBuffer DeviceBuffer { get; private set; }
        private bool _dirty = true;

        public string Name { get => this.DeviceBuffer.Name; set => this.DeviceBuffer.Name = value; }

        public bool IsFull => (this._freeListCount == 0);

        public int Id { get; private set; }

        public unsafe DataBuffer(GraphicsDevice gd, int blocksCount, int blockSize, ResourceLayout resourceLayout)
        {
            this.Id = ++DataBuffer._count;
            this._gd = gd;
            this.BlockSize = blockSize;
            this.BlocksCount = blocksCount;
            this.SizeInBytes = this.BlocksCount * this.BlockSize;
            this._data = Marshal.AllocHGlobal((int)this.SizeInBytes);
            Unsafe.InitBlockUnaligned((byte*)this._data, 0, (uint)this.SizeInBytes);
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

        public void UploadToGPU(CommandList commandList)
        {
            if (!this._dirty) return;
            commandList.UpdateBuffer(this.DeviceBuffer, 0, this._data, (uint)this.SizeInBytes);
            this._dirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataBlock RequestBlock()
        {
            if (this._freeListCount == 0) throw new Exception($"Buffer {this.Name} is full. This should not happen.");

            this._freeListCount--;
            return new DataBlock(this, this._freeList[this._freeListCount]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FreeBlock(int offset)
        {
            this._freeList[this._freeListCount] = offset;
            this._freeListCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(int offset, ref T data) where T : unmanaged
        {
            Marshal.StructureToPtr(data, this._data + offset, false);
            this._dirty = true;
        }

        private readonly System.Numerics.Matrix4x4[] _mats = new System.Numerics.Matrix4x4[1];

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(int offset) where T : struct
        {
            return Marshal.PtrToStructure<T>(this._data + offset);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(this._data);
        }
    }
}