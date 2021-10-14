using System;
using System.Runtime.CompilerServices;

namespace LifeSim.Rendering
{
    internal struct DataBlock
    {
        internal DataBuffer Buffer { get; set; }
        public int Offset { get; set; }

        public bool IsValid => this.Buffer != null;

        public int BlockSize => this.Buffer == null ? 0 : this.Buffer.BlockSize;

        public uint BlockIndex => (uint)(this.Offset / this.Buffer.BlockSize);

        internal DataBlock(DataBuffer buffer, int offset)
        {
            this.Buffer = buffer;
            this.Offset = offset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Read<T>() where T : unmanaged
        {
            return this.Buffer.Read<T>(this.Offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(ref T data) where T : unmanaged
        {
            this.Buffer.Write<T>(this.Offset, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write<T>(int offset, ref T data) where T : unmanaged
        {
            this.Buffer.Write<T>(this.Offset + offset, ref data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(ReadOnlySpan<T> data) where T : unmanaged
        {
            this.Buffer.WriteSpan<T>(this.Offset, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteSpan<T>(Span<T> data) where T : unmanaged
        {
            this.Buffer.WriteSpan<T>(this.Offset, data);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FreeBlock()
        {
            this.Buffer?.FreeBlock(this.Offset);
        }
    }
}