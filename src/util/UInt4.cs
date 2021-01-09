using System;
using System.Runtime.InteropServices;

namespace LifeSim.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UInt4
    {
        public uint X;
        public uint Y;
        public uint Z;
        public uint W;

        public UInt4(uint x, uint y, uint z, uint w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        public override bool Equals(object? obj)
        {
            return obj is UInt4 @int &&
                   X == @int.X &&
                   Y == @int.Y &&
                   Z == @int.Z &&
                   W == @int.W;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y, Z, W);
        }

        public override string? ToString()
        {
            return "<" + this.X + ", " + this.Y + ", " + this.Z + ", " + this.W + ">";
        }

        public static implicit operator UInt4(UShort4 v)
        {
            return new UInt4(v.X, v.Y, v.Z, v.Z);
        }
    }
}