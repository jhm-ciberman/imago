using System;
using System.Numerics;

namespace LifeSim.GLTF
{
    public class GLTFBuffer
    {
        public readonly byte[] _bytes;

        public GLTFBuffer(byte[] bytes)
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

        public Vector4 ReadVector4UShort(int offset)
        {
            Vector4 vector;
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
    }
}