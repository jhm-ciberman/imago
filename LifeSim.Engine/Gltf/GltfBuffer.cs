using System;
using System.Numerics;

namespace LifeSim.Engine.Gltf;

internal class GltfBuffer
{
    private readonly byte[] _bytes;

    internal GltfBuffer(byte[] bytes)
    {
        this._bytes = bytes;
    }

    public Vector4 ReadVector4(int offset)
    {
        Vector4 vector;
        vector.X = BitConverter.ToSingle(this._bytes, offset + 0);
        vector.Y = BitConverter.ToSingle(this._bytes, offset + 4);
        vector.Z = BitConverter.ToSingle(this._bytes, offset + 8);
        vector.W = BitConverter.ToSingle(this._bytes, offset + 12);
        return vector;
    }

    public Vector4UShort ReadUShort4(int offset)
    {
        Vector4UShort vector;
        vector.X = BitConverter.ToUInt16(this._bytes, offset + 0);
        vector.Y = BitConverter.ToUInt16(this._bytes, offset + 2);
        vector.Z = BitConverter.ToUInt16(this._bytes, offset + 4);
        vector.W = BitConverter.ToUInt16(this._bytes, offset + 6);
        return vector;
    }

    public Vector3 ReadVector3(int offset)
    {
        Vector3 vector;
        vector.X = BitConverter.ToSingle(this._bytes, offset + 0);
        vector.Y = BitConverter.ToSingle(this._bytes, offset + 4);
        vector.Z = BitConverter.ToSingle(this._bytes, offset + 8);
        return vector;
    }

    public Vector2 ReadVector2(int offset)
    {
        Vector2 vector;
        vector.X = BitConverter.ToSingle(this._bytes, offset + 0);
        vector.Y = BitConverter.ToSingle(this._bytes, offset + 4);
        return vector;
    }

    public ushort ReadUShort(int offset)
    {
        return BitConverter.ToUInt16(this._bytes, offset);
    }

    public uint ReadUInt(int offset)
    {
        return BitConverter.ToUInt32(this._bytes, offset);
    }

    public byte ReadByte(int offset)
    {
        return this._bytes[offset];
    }

    public float ReadFloat(int offset)
    {
        return BitConverter.ToSingle(this._bytes, offset);
    }

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