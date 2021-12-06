using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace LifeSim.Engine.GLTF
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector4UShort
    {
        public ushort X;
        public ushort Y;
        public ushort Z;
        public ushort W;

        public Vector4UShort(ushort x, ushort y, ushort z, ushort w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public override bool Equals(object? obj)
        {
            return obj is Vector4UShort other &&
                   this.X == other.X &&
                   this.Y == other.Y &&
                   this.Z == other.Z &&
                   this.W == other.W;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.X, this.Y, this.Z, this.W);
        }

        public override string? ToString()
        {
            return "<" + this.X + ", " + this.Y + ", " + this.Z + ", " + this.W + ">";
        }

        public static implicit operator Vector4(Vector4UShort v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }
    }
}