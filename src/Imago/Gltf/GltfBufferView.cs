using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Numerics;

namespace Imago.Gltf;

internal class GltfBufferView : IGltfBufferView
{
    private readonly GltfBuffer _buffer;
    private readonly int _byteOffset;
    private readonly int _byteStride;

    public GltfBufferView(GltfBuffer buffer, int byteOffset, int? byteStride)
    {
        this._buffer = buffer;
        this._byteOffset = byteOffset;
        this._byteStride = byteStride ?? 0;
    }

    private T[] Read<T>(int offset, int count, Func<int, T> reader) where T : struct
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

    public Vector2[] ReadVector2Array(int offset, int count)
    {
        return this.Read<Vector2>(offset, count, this._buffer.ReadVector2);
    }

    public Vector3[] ReadVector3Array(int offset, int count)
    {
        return this.Read<Vector3>(offset, count, this._buffer.ReadVector3);
    }

    public Vector4[] ReadVector4Array(int offset, int count)
    {
        return this.Read<Vector4>(offset, count, this._buffer.ReadVector4);
    }

    public Vector4UShort[] ReadUShort4Array(int offset, int count)
    {
        return this.Read<Vector4UShort>(offset, count, this._buffer.ReadUShort4);
    }

    public ushort[] ReadUShortArray(int offset, int count)
    {
        return this.Read<ushort>(offset, count, this._buffer.ReadUShort);
    }

    public uint[] ReadUIntArray(int offset, int count)
    {
        return this.Read<uint>(offset, count, this._buffer.ReadUInt);
    }

    public byte[] ReadByteArray(int offset, int count)
    {
        return this.Read<byte>(offset, count, this._buffer.ReadByte);
    }

    public float[] ReadFloatArray(int offset, int count)
    {
        return this.Read<float>(offset, count, this._buffer.ReadFloat);
    }

    public sbyte[] ReadSByteArray(int offset, int count)
    {
        return this.Read<sbyte>(offset, count, this._buffer.ReadSByte);
    }

    public short[] ReadShortArray(int offset, int count)
    {
        return this.Read<short>(offset, count, this._buffer.ReadShort);
    }

    public Quaternion[] ReadQuaternionArray(int offset, int count)
    {
        return this.Read<Quaternion>(offset, count, this._buffer.ReadQuaternion);
    }

    public Matrix4x4[] ReadMatrix4x4Array(int offset, int count)
    {
        return this.Read<Matrix4x4>(offset, count, this._buffer.ReadMatrix4x4);
    }
}
