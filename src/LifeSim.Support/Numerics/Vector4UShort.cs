using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace LifeSim.Support.Numerics;

/// <summary>
/// A 4-component vector of unsigned shorts.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct Vector4UShort
{
    /// <summary>
    /// The X component of the vector.
    /// </summary>
    public ushort X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    public ushort Y;

    /// <summary>
    /// The Z component of the vector.
    /// </summary>
    public ushort Z;

    /// <summary>
    /// The W component of the vector.
    /// </summary>
    public ushort W;

    /// <summary>
    /// Constructs a new Vector4UShort.
    /// </summary>
    /// <param name="x">The X component of the vector.</param>
    /// <param name="y">The Y component of the vector.</param>
    /// <param name="z">The Z component of the vector.</param>
    /// <param name="w">The W component of the vector.</param>
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

    public static bool operator ==(Vector4UShort left, Vector4UShort right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector4UShort left, Vector4UShort right)
    {
        return !(left == right);
    }
}
