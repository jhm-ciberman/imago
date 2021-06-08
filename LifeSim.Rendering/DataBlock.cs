using System;
using System.Runtime.CompilerServices;

namespace LifeSim.Engine.Rendering
{
    internal struct DataBlock
    {
        internal DataBuffer buffer;
        public int offset;

        public bool isValid => this.buffer != null;

        public int blockSize => this.buffer == null ? 0 : this.buffer.blockSize;

        public uint blockIndex => (uint) (this.offset / this.buffer.blockSize);

        internal DataBlock(DataBuffer buffer, int offset)
        {
            this.buffer = buffer;
            this.offset = offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            return this.buffer.Read<T>(this.offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(ref T data) where T : unmanaged
        {
            this.buffer.Write<T>(this.offset, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(int offset, ref T data) where T : unmanaged
        {
            this.buffer.Write<T>(this.offset + offset, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            this.buffer.WriteSpan<T>(this.offset, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(Span<T> data) where T : unmanaged
        {
            this.buffer.WriteSpan<T>(this.offset, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FreeBlock()
        {
            this.buffer?.FreeBlock(this.offset);
        }
    }
}