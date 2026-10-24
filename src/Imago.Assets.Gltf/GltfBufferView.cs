using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Support.Numerics;

namespace Imago.Assets.Gltf;

/// <summary>
/// Represents a glTF buffer view, providing methods to read arrays of various data types from a <see cref="GltfBuffer"/>.
/// </summary>
internal class GltfBufferView : IGltfBufferView
{
    private readonly GltfBuffer _buffer;
    private readonly int _byteOffset;
    private readonly int _byteStride;

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfBufferView"/> class.
    /// </summary>
    /// <param name="buffer">The <see cref="GltfBuffer"/> containing the data.</param>
    /// <param name="byteOffset">The byte offset within the buffer where the buffer view begins.</param>
    /// <param name="byteStride">The stride, in bytes, between vertex attributes. If null, the stride is tightly packed.</param>
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

    /// <summary>
    /// Reads an array of <see cref="Vector2"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Vector2"/> elements to read.</param>
    /// <returns>An array of <see cref="Vector2"/>.</returns>
    public Vector2[] ReadVector2Array(int offset, int count)
    {
        return this.Read<Vector2>(offset, count, this._buffer.ReadVector2);
    }

    /// <summary>
    /// Reads an array of <see cref="Vector3"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Vector3"/> elements to read.</param>
    /// <returns>An array of <see cref="Vector3"/>.</returns>
    public Vector3[] ReadVector3Array(int offset, int count)
    {
        return this.Read<Vector3>(offset, count, this._buffer.ReadVector3);
    }

    /// <summary>
    /// Reads an array of <see cref="Vector4"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Vector4"/> elements to read.</param>
    /// <returns>An array of <see cref="Vector4"/>.</returns>
    public Vector4[] ReadVector4Array(int offset, int count)
    {
        return this.Read<Vector4>(offset, count, this._buffer.ReadVector4);
    }

    /// <summary>
    /// Reads an array of <see cref="Vector4UShort"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Vector4UShort"/> elements to read.</param>
    /// <returns>An array of <see cref="Vector4UShort"/>.</returns>
    public Vector4UShort[] ReadUShort4Array(int offset, int count)
    {
        return this.Read<Vector4UShort>(offset, count, this._buffer.ReadUShort4);
    }

    /// <summary>
    /// Reads an array of unsigned 16-bit integers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="ushort"/> elements to read.</param>
    /// <returns>An array of <see cref="ushort"/>.</returns>
    public ushort[] ReadUShortArray(int offset, int count)
    {
        return this.Read<ushort>(offset, count, this._buffer.ReadUShort);
    }

    /// <summary>
    /// Reads an array of unsigned 32-bit integers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="uint"/> elements to read.</param>
    /// <returns>An array of <see cref="uint"/>.</returns>
    public uint[] ReadUIntArray(int offset, int count)
    {
        return this.Read<uint>(offset, count, this._buffer.ReadUInt);
    }

    /// <summary>
    /// Reads an array of bytes from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="byte"/> elements to read.</param>
    /// <returns>An array of <see cref="byte"/>.</returns>
    public byte[] ReadByteArray(int offset, int count)
    {
        return this.Read<byte>(offset, count, this._buffer.ReadByte);
    }

    /// <summary>
    /// Reads an array of single-precision floating-point numbers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="float"/> elements to read.</param>
    /// <returns>An array of <see cref="float"/>.</returns>
    public float[] ReadFloatArray(int offset, int count)
    {
        return this.Read<float>(offset, count, this._buffer.ReadFloat);
    }

    /// <summary>
    /// Reads an array of signed 8-bit integers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="sbyte"/> elements to read.</param>
    /// <returns>An array of <see cref="sbyte"/>.</returns>
    public sbyte[] ReadSByteArray(int offset, int count)
    {
        return this.Read<sbyte>(offset, count, this._buffer.ReadSByte);
    }

    /// <summary>
    /// Reads an array of signed 16-bit integers from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="short"/> elements to read.</param>
    /// <returns>An array of <see cref="short"/>.</returns>
    public short[] ReadShortArray(int offset, int count)
    {
        return this.Read<short>(offset, count, this._buffer.ReadShort);
    }

    /// <summary>
    /// Reads an array of <see cref="Quaternion"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Quaternion"/> elements to read.</param>
    /// <returns>An array of <see cref="Quaternion"/>.</returns>
    public Quaternion[] ReadQuaternionArray(int offset, int count)
    {
        return this.Read<Quaternion>(offset, count, this._buffer.ReadQuaternion);
    }

    /// <summary>
    /// Reads an array of <see cref="Matrix4x4"/> from the buffer view.
    /// </summary>
    /// <param name="offset">The byte offset within the buffer view from which to start reading.</param>
    /// <param name="count">The number of <see cref="Matrix4x4"/> elements to read.</param>
    /// <returns>An array of <see cref="Matrix4x4"/>.</returns>
    public Matrix4x4[] ReadMatrix4x4Array(int offset, int count)
    {
        return this.Read<Matrix4x4>(offset, count, this._buffer.ReadMatrix4x4);
    }
}
