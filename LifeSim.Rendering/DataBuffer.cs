using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Rendering
{
    internal partial class DataBuffer : IDisposable
    {
        private static int _count = 0;
        private IntPtr _data;

        public int blocksCount { get; private set; }
        public int blockSize { get; private set; }
        public int sizeInBytes { get; private set; }

        private int[] _freeList;
        private int _freeListCount = 0;

        private GraphicsDevice _gd;
        private ResourceLayout _resourceLayout;
        public ResourceSet resourceSet { get; private set; }
        public DeviceBuffer deviceBuffer { get; private set; }
        private bool _dirty = true;

        public string name { get => this.deviceBuffer.Name; set { this.deviceBuffer.Name = value; } }

        public bool isFull => (this._freeListCount == 0);

        public int id { get; private set; }

        public unsafe DataBuffer(GraphicsDevice gd, int blocksCount, int blockSize, ResourceLayout resourceLayout)
        {
            this.id = ++DataBuffer._count;
            this._gd = gd;
            this.blockSize = blockSize;
            this.blocksCount = blocksCount;
            this.sizeInBytes = this.blocksCount * this.blockSize;
            this._data = Marshal.AllocHGlobal((int) this.sizeInBytes);
            Unsafe.InitBlockUnaligned((byte*)this._data, 0, (uint) this.sizeInBytes);
            this._dirty = true;
            this._resourceLayout = resourceLayout;

            this.deviceBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint) this.sizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic
            ));

            this.resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                this._resourceLayout, this.deviceBuffer
            ));

            this._freeList = new int[blocksCount];
            for (int i = 0; i < blocksCount; i++) {
                this._freeList[i] = (this.blocksCount - i - 1) * this.blockSize;
            }
            this._freeListCount = this.blocksCount;
        }

        public void UploadToGPU(CommandList commandList)
        {
            if (! this._dirty) return;
            commandList.UpdateBuffer(this.deviceBuffer, 0, this._data, (uint) this.sizeInBytes);
            this._dirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DataBlock RequestBlock()
        {
            if (this._freeListCount == 0) throw new Exception($"Buffer {this.name} is full. This should not happen.");

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

        private System.Numerics.Matrix4x4[] _mats = new System.Numerics.Matrix4x4[1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void WriteSpan<T>(int offset, ReadOnlySpan<T> data) where T : unmanaged
        {
            fixed(T* ptr = data) {
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