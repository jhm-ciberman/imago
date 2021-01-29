using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace LifeSim.Engine.Rendering
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UShort4
    {
        public ushort x;
        public ushort y;
        public ushort z;
        public ushort w;

        public UShort4(ushort x, ushort y, ushort z, ushort w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override bool Equals(object? obj)
        {
            return obj is UShort4 other &&
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

        public static implicit operator Vector4(UShort4 v)
        {
            return new Vector4(v.x, v.y, v.z, v.w);
        }
    }
}