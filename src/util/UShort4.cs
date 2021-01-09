using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace LifeSim.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UShort4
    {
        public ushort X;
        public ushort Y;
        public ushort Z;
        public ushort W;

        public UShort4(ushort x, ushort y, ushort z, ushort w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public override bool Equals(object? obj)
        {
            return obj is UShort4 @short &&
                   X == @short.X &&
                   Y == @short.Y &&
                   Z == @short.Z &&
                   W == @short.W;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        public override string? ToString()
        {
            return "<" + this.X + ", " + this.Y + ", " + this.Z + ", " + this.W + ">";
        }

        public static implicit operator Vector4(UShort4 v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }
    }
}