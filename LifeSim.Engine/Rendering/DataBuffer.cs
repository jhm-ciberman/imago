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
            public int offset;

            public Block(DataBuffer buffer, int offset)
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
                this.offset = -1;
            }
        }

        private IntPtr _data;

        private int _blocksCount;
        private int _blockSize;
        public int sizeInBytes => (this._blocksCount * this._blockSize);

        private int[] _freeList;
        private int _freeListCount = 0;

        private GraphicsDevice _gd;
        private ResourceLayout _resourceLayout;
        public ResourceSet resourceSet { get; private set; }
        public DeviceBuffer deviceBuffer { get; private set; }
        private bool _dirty = true;

        public event Action? onResourceSetChanged;

        public string name { get => this.deviceBuffer.Name; set { this.deviceBuffer.Name = value; } }

        public DataBuffer(GraphicsDevice gd, int blocksCount, int blockSize, ResourceLayout resourceLayout)
        {
            this._gd = gd;
            this._blockSize = blockSize;
            this._blocksCount = blocksCount;
            this._data = Marshal.AllocHGlobal((int) this.sizeInBytes);
            this._resourceLayout = resourceLayout;

            this.deviceBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(
                (uint)this.sizeInBytes, 
                BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic, 
                (uint) this._blockSize
            ));

            this.resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this.deviceBuffer));
            System.Console.WriteLine("StructuredBufferMinOffsetAlignment: " + this._gd.StructuredBufferMinOffsetAlignment);
            System.Console.WriteLine("UniformBufferMinOffsetAlignment: " + this._gd.UniformBufferMinOffsetAlignment);
            this._freeList = new int[blocksCount];
            for (int i = 0; i < blocksCount; i++) {
                this._freeList[i] = (this._blocksCount - i - 1) * this._blockSize;
            }
            this._freeListCount = this._blocksCount;
        }

        public void Resize(int newBlockCount)
        {
            if (this._blocksCount == newBlockCount) return;
            int oldBlocksCount = this._blocksCount;

            this._blocksCount = newBlockCount;
            int newSize = (newBlockCount * this._blockSize);
            this._data = Marshal.ReAllocHGlobal(this._data, (IntPtr) newSize);
            Array.Resize(ref this._freeList, newBlockCount);

            int extraBlocks = newBlockCount - oldBlocksCount;
            if (extraBlocks > 0) {
                for (int i = 0; i < extraBlocks; i++) {
                    this._freeList[this._freeListCount + i] = (newBlockCount - i - 1) * this._blockSize;
                }
            }
            this._freeListCount += extraBlocks;

            this._gd.DisposeWhenIdle(this.deviceBuffer);
            this.deviceBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)this.sizeInBytes, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this.resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this.deviceBuffer));
            this.onResourceSetChanged?.Invoke();
        }

        public void UploadToGPU(CommandList commandList)
        {
            if (! this._dirty) return;
            commandList.UpdateBuffer(this.deviceBuffer, 0, this._data, (uint) this.sizeInBytes);
            this._dirty = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block RequestBlock()
        {
            if (this._freeListCount == 0) {
                throw new Exception("No new Blocks available in the DataBuffer. This should never happen.");
            }

            this._freeListCount--;
            Console.WriteLine($"Request block from {this.name}: {this._freeList[this._freeListCount]}");
            return new Block(this, this._freeList[this._freeListCount]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FreeBlock(int offset)
        {
            this._freeList[this._freeListCount] = offset;
            this._freeListCount++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(int offset, ref T data) where T : struct
        {
            if (offset < 0) {
                throw new Exception("Trying to write to an invalid data block");
            }
            Console.WriteLine($"Write to data {this.name}");
            Marshal.StructureToPtr(data, this._data + offset, false);
            this._dirty = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>(int offset) where T : struct
        {
            if (offset < 0) {
                throw new Exception("Trying to read from an invalid data block");
            }
            return Marshal.PtrToStructure<T>(this._data + offset);
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(this._data);
        }
    }
}