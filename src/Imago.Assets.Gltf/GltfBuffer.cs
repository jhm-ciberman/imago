using System;
using System.Numerics;
using Imago.Support.Numerics;

namespace Imago.Assets.Gltf;

/// <summary>
/// Represents a glTF buffer, providing methods to read various data types from its byte array.
/// </summary>
internal class GltfBuffer
{
    private readonly byte[] _bytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="GltfBuffer"/> class with the specified byte array.
    /// </summary>
    /// <param name="bytes">The byte array containing the buffer data.</param>
    internal GltfBuffer(byte[] bytes)
    {
        this._bytes = bytes;
    }

    /// <summary>
    /// Reads a <see cref="Vector4"/> from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the <see cref="Vector4"/>.</param>
    /// <returns>The <see cref="Vector4"/> read from the buffer.</returns>
    public Vector4 ReadVector4(int offset)
    {
        Vector4 vector;
        vector.X = BitConverter.ToSingle(this._bytes, offset + 0);
        vector.Y = BitConverter.ToSingle(this._bytes, offset + 4);
        vector.Z = BitConverter.ToSingle(this._bytes, offset + 8);
        vector.W = BitConverter.ToSingle(this._bytes, offset + 12);
        return vector;
    }

    /// <summary>
    /// Reads a <see cref="Vector4UShort"/> from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the <see cref="Vector4UShort"/>.</param>
    /// <returns>The <see cref="Vector4UShort"/> read from the buffer.</returns>
    public Vector4UShort ReadUShort4(int offset)
    {
        Vector4UShort vector;
        vector.X = BitConverter.ToUInt16(this._bytes, offset + 0);
        vector.Y = BitConverter.ToUInt16(this._bytes, offset + 2);
        vector.Z = BitConverter.ToUInt16(this._bytes, offset + 4);
        vector.W = BitConverter.ToUInt16(this._bytes, offset + 6);
        return vector;
    }

    /// <summary>
    /// Reads a <see cref="Vector3"/> from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the <see cref="Vector3"/>.</param>
    /// <returns>The <see cref="Vector3"/> read from the buffer.</returns>
    public Vector3 ReadVector3(int offset)
    {
        Vector3 vector;
        vector.X = BitConverter.ToSingle(this._bytes, offset + 0);
        vector.Y = BitConverter.ToSingle(this._bytes, offset + 4);
        vector.Z = BitConverter.ToSingle(this._bytes, offset + 8);
        return vector;
    }

    /// <summary>
    /// Reads a <see cref="Vector2"/> from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the <see cref="Vector2"/>.</param>
    /// <returns>The <see cref="Vector2"/> read from the buffer.</returns>
    public Vector2 ReadVector2(int offset)
    {
        Vector2 vector;
        vector.X = BitConverter.ToSingle(this._bytes, offset + 0);
        vector.Y = BitConverter.ToSingle(this._bytes, offset + 4);
        return vector;
    }

    /// <summary>
    /// Reads an unsigned 16-bit integer from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the unsigned short.</param>
    /// <returns>The <see cref="ushort"/> read from the buffer.</returns>
    public ushort ReadUShort(int offset)
    {
        return BitConverter.ToUInt16(this._bytes, offset);
    }

    /// <summary>
    /// Reads an unsigned 32-bit integer from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the unsigned integer.</param>
    /// <returns>The <see cref="uint"/> read from the buffer.</returns>
    public uint ReadUInt(int offset)
    {
        return BitConverter.ToUInt32(this._bytes, offset);
    }

    /// <summary>
    /// Reads a byte from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the byte.</param>
    /// <returns>The <see cref="byte"/> read from the buffer.</returns>
    public byte ReadByte(int offset)
    {
        return this._bytes[offset];
    }

    /// <summary>
    /// Reads a single-precision floating-point number from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the float.</param>
    /// <returns>The <see cref="float"/> read from the buffer.</returns>
    public float ReadFloat(int offset)
    {
        return BitConverter.ToSingle(this._bytes, offset);
    }

    /// <summary>
    /// Reads a <see cref="Matrix4x4"/> from the buffer at the specified byte offset.
    /// </summary>
    /// <param name="offset">The byte offset from which to read the <see cref="Matrix4x4"/>.</param>
    /// <returns>The <see cref="Matrix4x4"/> read from the buffer.</returns>
    public Matrix4x4 ReadMatrix4x4(int offset)
    {
        Matrix4x4 mat = new Matrix4x4();

        mat.M11 = BitConverter.ToSingle(this._bytes, offset + 0);
        mat.M12 = BitConverter.ToSingle(this._bytes, offset + 4);
        mat.M13 = BitConverter.ToSingle(this._bytes, offset + 8);
        mat.M14 = BitConverter.ToSingle(this._bytes, offset + 12);

        mat.M21 = BitConverter.ToSingle(this._bytes, offset + 16);
        mat.M22 = BitConverter.ToSingle(this._bytes, offset + 20);
        mat.M23 = BitConverter.ToSingle(this._bytes, offset + 24);
        mat.M24 = BitConverter.ToSingle(this._bytes, offset + 28);

        mat.M31 = BitConverter.ToSingle(this._bytes, offset + 32);
        mat.M32 = BitConverter.ToSingle(this._bytes, offset + 36);
        mat.M33 = BitConverter.ToSingle(this._bytes, offset + 40);
        mat.M34 = BitConverter.ToSingle(this._bytes, offset + 44);

        mat.M41 = BitConverter.ToSingle(this._bytes, offset + 48);
        mat.M42 = BitConverter.ToSingle(this._bytes, offset + 52);
        mat.M43 = BitConverter.ToSingle(this._bytes, offset + 56);
        mat.M44 = BitConverter.ToSingle(this._bytes, offset + 60);
        return mat;
    }

    internal Vector4UShort ReadByte4(int offset)
    {
        Vector4UShort vector;
        vector.X = this._bytes[offset + 0];
        vector.Y = this._bytes[offset + 1];
        vector.Z = this._bytes[offset + 2];
        vector.W = this._bytes[offset + 3];
        return vector;
    }

    internal Quaternion ReadQuaternion(int offset)
    {
        Quaternion quat;
        quat.X = BitConverter.ToSingle(this._bytes, offset + 0);
        quat.Y = BitConverter.ToSingle(this._bytes, offset + 4);
        quat.Z = BitConverter.ToSingle(this._bytes, offset + 8);
        quat.W = BitConverter.ToSingle(this._bytes, offset + 12);
        return quat;
    }

    internal sbyte ReadSByte(int offset)
    {
        return unchecked((sbyte)this._bytes[offset]);
    }

    internal short ReadShort(int offset)
    {
        return BitConverter.ToInt16(this._bytes, offset);
    }
}
