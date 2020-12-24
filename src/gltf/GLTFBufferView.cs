using System.Numerics;
using System.Runtime.InteropServices;

namespace LifeSim.GLTF
{
    public interface IGLTFBufferView
    {
        Vector2[] ReadVector2Array(int offset, int count);
        Vector3[] ReadVector3Array(int offset, int count);
        Vector4[] ReadVector4Array(int offset, int count);
        ushort[] ReadUShortArray(int offset, int count);
        Vector4[] ReadUShortVector4Array(int offset, int count);
        uint[] ReadUIntArray(int offset, int count);
        byte[] ReadByteArray(int offset, int count);
    }

    public class GLTFBufferViewZeroed : IGLTFBufferView
    {

        public Vector2[] ReadVector2Array(int offset, int count)       => new Vector2[count];
        public Vector3[] ReadVector3Array(int offset, int count)       => new Vector3[count];
        public Vector4[] ReadVector4Array(int offset, int count)       => new Vector4[count];
        public ushort[]  ReadUShortArray(int offset, int count)        => new ushort[count];
        public Vector4[] ReadUShortVector4Array(int offset, int count) => new Vector4[count];
        public uint[]    ReadUIntArray(int offset, int count)          => new uint[count];
        public byte[]    ReadByteArray(int offset, int count)          => new byte[count];
    }

    public class GLTFBufferView : IGLTFBufferView
    {
        private GLTFBuffer _buffer;
        private int _byteLength;
        private int _byteOffset;
        private int _byteStride;

        public GLTFBufferView(GLTFBuffer buffer, int byteLength, int byteOffset, int? byteStride)
        {
            this._buffer = buffer;
            this._byteLength = byteLength;
            this._byteOffset = byteOffset;
            this._byteStride = byteStride ?? 0;

        }

        public T[] _Read<T>(int offset, int count, System.Func<int, T> reader) where T : struct
        {
            var arr = new T[count];
            var stride = this._byteStride == 0 ? Marshal.SizeOf(typeof(T)) : this._byteStride;
            int finalOffset = offset + this._byteOffset;
            for (int i = 0; i < count; i++) {
                arr[i] = reader.Invoke(finalOffset + i * stride);
            }
            return arr;
        }

        public Vector2[] ReadVector2Array(int offset, int count)       => this._Read<Vector2>(offset, count, this._buffer.ReadVector2);
        public Vector3[] ReadVector3Array(int offset, int count)       => this._Read<Vector3>(offset, count, this._buffer.ReadVector3);
        public Vector4[] ReadVector4Array(int offset, int count)       => this._Read<Vector4>(offset, count, this._buffer.ReadVector4);
        
        public Vector4[] ReadUShortVector4Array(int offset, int count) => this._Read<Vector4>(offset, count, this._buffer.ReadVector4UShort);
        public ushort[]  ReadUShortArray(int offset, int count)        => this._Read<ushort>(offset, count, this._buffer.ReadUShort);
        public uint[]    ReadUIntArray(int offset, int count)          => this._Read<uint>(offset, count, this._buffer.ReadUInt);
        public byte[]    ReadByteArray(int offset, int count)          => this._Read<byte>(offset, count, this._buffer.ReadByte);
    }
}