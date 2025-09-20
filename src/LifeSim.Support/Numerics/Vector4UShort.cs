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

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Vector4UShort other &&
               this.X == other.X &&
               this.Y == other.Y &&
               this.Z == other.Z &&
               this.W == other.W;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(this.X, this.Y, this.Z, this.W);
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return "<" + this.X + ", " + this.Y + ", " + this.Z + ", " + this.W + ">";
    }

    /// <summary>
    /// Implicitly converts a <see cref="Vector4UShort"/> to a <see cref="Vector4"/>.
    /// </summary>
    /// <param name="v">The vector to convert.</param>
    /// <returns>A <see cref="Vector4"/> with the same component values.</returns>
    public static implicit operator Vector4(Vector4UShort v)
    {
        return new Vector4(v.X, v.Y, v.Z, v.W);
    }

    /// <summary>
    /// Determines whether two <see cref="Vector4UShort"/> instances are equal.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>true if the vectors are equal; otherwise, false.</returns>
    public static bool operator ==(Vector4UShort left, Vector4UShort right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Vector4UShort"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>true if the vectors are not equal; otherwise, false.</returns>
    public static bool operator !=(Vector4UShort left, Vector4UShort right)
    {
        return !(left == right);
    }
}
