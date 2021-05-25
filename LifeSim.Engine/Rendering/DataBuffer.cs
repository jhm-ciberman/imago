using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class DataBuffer : IDisposable
    {
        public struct Block
        {
            public DataBuffer buffer;
            public uint offset;

            public Block(DataBuffer buffer, uint offset)
            {
                this.buffer = buffer;
                this.offset = offset;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public T Read<T>() where T : struct
            {
                return this.buffer.Read<T>(this.offset);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Write<T>(ref T data) where T : struct
            {
                this.buffer.Write<T>(this.offset, ref data);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void FreeBlock()
            {
                this.buffer.FreeBlock(this.offset);
            }
        }

        private static int _count = 0;
        private IntPtr _data;

        public int blocksCount { get; private set; }
        public int blockSize { get; private set; }
        public int sizeInBytes { get; private set; }

        private uint[] _freeList;
        private int _freeListCount = 0;

        private GraphicsDevice _gd;
        private ResourceLayout _resourceLayout;
        public ResourceSet resourceSet { get; private set; }
        public DeviceBuffer deviceBuffer { get; private set; }
        private bool _dirty = true;

        public event Action? onResourceSetChanged;

        public string name { get => this.deviceBuffer.Name; set { this.deviceBuffer.Name = value; } }

        public int id { get; private set; }

        public DataBuffer(GraphicsDevice gd, int blocksCount, int blockSize, ResourceLayout resourceLayout)
        {
            this.id = ++DataBuffer._count;
            this._gd = gd;
            this.blockSize = blockSize;
            this.blocksCount = blocksCount;
            this.sizeInBytes = this.blocksCount * this.blockSize;
            this._data = Marshal.AllocHGlobal((int) this.sizeInBytes);
            this._resourceLayout = resourceLayout;

            this.deviceBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint) this.sizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic
            ));

            this.resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                this._resourceLayout, this.deviceBuffer
            ));

            this._freeList = new uint[blocksCount];
            for (int i = 0; i < blocksCount; i++) {
                this._freeList[i] = (uint)(this.blocksCount - i - 1);
            }
            this._freeListCount = this.blocksCount;
        }

        public void UploadToGPU(CommandList commandList)
        {
            if (! this._dirty) return;
            commandList.UpdateBuffer(this.deviceBuffer, 0, this._data, (uint) this.sizeInBytes);
            this._dirty = false;
        }

        public bool isFull => (this._freeListCount == 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block RequestBlock()
        {
            if (this._freeListCount == 0) throw new Exception($"Buffer {this.name} is full. This should not happen.");

            this._freeListCount--;
            return new Block(this, this._freeList[this._freeListCount]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FreeBlock(uint offset)
        {
            this._freeList[this._freeListCount] = offset;
            this._freeListCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(uint offset, ref T data) where T : struct
        {
            /*
            if (offset < 0) {
                throw new Exception("Trying to write to an invalid data block");
            }
            if (Marshal.SizeOf<T>() > this._blockSize) {
                throw new Exception($"The size of {typeof(T)} is bigger than the block size");
            }
            */
            
            Marshal.StructureToPtr(data, this._data + (int) offset * this.blockSize, false);
            this._dirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(uint offset) where T : struct
        {
            return Marshal.PtrToStructure<T>(this._data + (int) offset * this.blockSize);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(this._data);
        }
    }
}