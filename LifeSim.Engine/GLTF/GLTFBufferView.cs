using System.Numerics;
using System.Runtime.InteropServices;

namespace LifeSim.Engine.GLTF
{
    internal interface IGLTFBufferView
    {
        Vector2[] ReadVector2Array(int offset, int count);
        Vector3[] ReadVector3Array(int offset, int count);
        Vector4[] ReadVector4Array(int offset, int count);
        ushort[] ReadUShortArray(int offset, int count);
        Vector4UShort[] ReadUShort4Array(int offset, int count);
        uint[] ReadUIntArray(int offset, int count);
        byte[] ReadByteArray(int offset, int count);
        float[] ReadFloatArray(int offset, int count);
        Matrix4x4[] ReadMatrix4x4Array(int offset, int count);
        Quaternion[] ReadQuaternionArray(int offset, int count);
        sbyte[] ReadSByteArray(int offset, int count);
        short[] ReadShortArray(int offset, int count);
    }

    internal class GLTFBufferViewZeroed : IGLTFBufferView
    {

        public Vector2[] ReadVector2Array(int offset, int count) => new Vector2[count];
        public Vector3[] ReadVector3Array(int offset, int count) => new Vector3[count];
        public Vector4[] ReadVector4Array(int offset, int count) => new Vector4[count];
        public ushort[] ReadUShortArray(int offset, int count) => new ushort[count];
        public Vector4UShort[] ReadUShort4Array(int offset, int count) => new Vector4UShort[count];
        public uint[] ReadUIntArray(int offset, int count) => new uint[count];
        public byte[] ReadByteArray(int offset, int count) => new byte[count];
        public float[] ReadFloatArray(int offset, int count) => new float[count];
        public sbyte[] ReadSByteArray(int offset, int count) => new sbyte[count];
        public short[] ReadShortArray(int offset, int count) => new short[count];
        public Quaternion[] ReadQuaternionArray(int offset, int count) => new Quaternion[count];

        public Matrix4x4[] ReadMatrix4x4Array(int offset, int count)
        {
            Matrix4x4[] matrices = new Matrix4x4[count];
            for (int i = 0; i < matrices.Length; i++)
            {
                matrices[i] = Matrix4x4.Identity;
            }
            return matrices;
        }
    }

    internal class GLTFBufferView : IGLTFBufferView
    {
        private readonly GLTFBuffer _buffer;
        private readonly int _byteOffset;
        private readonly int _byteStride;

        public GLTFBufferView(GLTFBuffer buffer, int byteOffset, int? byteStride)
        {
            this._buffer = buffer;
            this._byteOffset = byteOffset;
            this._byteStride = byteStride ?? 0;
        }

        public T[] _Read<T>(int offset, int count, System.Func<int, T> reader) where T : struct
        {
            var arr = new T[count];
            var stride = this._byteStride == 0 ? Marshal.SizeOf(typeof(T)) : this._byteStride;
            int finalOffset = offset + this._byteOffset;
            for (int i = 0; i < count; i++)
            {
                arr[i] = reader.Invoke(finalOffset + i * stride);
            }
            return arr;
        }

        public Vector2[] ReadVector2Array(int offset, int count) => this._Read<Vector2>(offset, count, this._buffer.ReadVector2);
        public Vector3[] ReadVector3Array(int offset, int count) => this._Read<Vector3>(offset, count, this._buffer.ReadVector3);
        public Vector4[] ReadVector4Array(int offset, int count) => this._Read<Vector4>(offset, count, this._buffer.ReadVector4);

        public Vector4UShort[] ReadUShort4Array(int offset, int count) => this._Read<Vector4UShort>(offset, count, this._buffer.ReadUShort4);
        public ushort[] ReadUShortArray(int offset, int count) => this._Read<ushort>(offset, count, this._buffer.ReadUShort);
        public uint[] ReadUIntArray(int offset, int count) => this._Read<uint>(offset, count, this._buffer.ReadUInt);
        public byte[] ReadByteArray(int offset, int count) => this._Read<byte>(offset, count, this._buffer.ReadByte);
        public float[] ReadFloatArray(int offset, int count) => this._Read<float>(offset, count, this._buffer.ReadFloat);
        public sbyte[] ReadSByteArray(int offset, int count) => this._Read<sbyte>(offset, count, this._buffer.ReadSByte);
        public short[] ReadShortArray(int offset, int count) => this._Read<short>(offset, count, this._buffer.ReadShort);

        public Quaternion[] ReadQuaternionArray(int offset, int count) => this._Read<Quaternion>(offset, count, this._buffer.ReadQuaternion);

        public Matrix4x4[] ReadMatrix4x4Array(int offset, int count) => this._Read<Matrix4x4>(offset, count, this._buffer.ReadMatrix4x4);
    }
}