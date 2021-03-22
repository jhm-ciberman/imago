using System;
using System.Runtime.InteropServices;

namespace LifeSim.Engine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UInt4
    {
        public uint x;
        public uint y;
        public uint z;
        public uint w;

        public UInt4(uint x, uint y, uint z, uint w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override bool Equals(object? obj)
        {
            return obj is UInt4 other &&
                   this.x == other.x &&
                   this.y == other.y &&
                   this.z == other.z &&
                   this.w == other.w;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.x, this.y, this.z, this.w);
        }

        public override string? ToString()
        {
            return "<" + this.x + ", " + this.y + ", " + this.z + ", " + this.w + ">";
        }

        public static implicit operator UInt4(UShort4 v)
        {
            return new UInt4(v.x, v.y, v.z, v.z);
        }
    }
}