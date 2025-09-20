using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Represents a 3D vector with integer components.
/// </summary>
public struct Vector3Int : IEquatable<Vector3Int>
{
    /// <summary>
    /// The X component of the vector.
    /// </summary>
    public int X;

    /// <summary>
    /// The Y component of the vector.
    /// </summary>
    public int Y;

    /// <summary>
    /// The Z component of the vector.
    /// </summary>
    public int Z;

    /// <summary>
    /// Gets a vector with all components set to 1.
    /// </summary>
    public static Vector3Int One => new Vector3Int(1, 1, 1);

    /// <summary>
    /// Gets a vector with all components set to 0.
    /// </summary>
    public static Vector3Int Zero => new Vector3Int(0, 0, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3Int"/> struct.
    /// </summary>
    /// <param name="x">The X component of the vector.</param>
    /// <param name="y">The Y component of the vector.</param>
    /// <param name="z">The Z component of the vector.</param>
    public Vector3Int(int x, int y, int z)
    {
        this.X = x;
        this.Y = y;
        this.Z = z;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3Int"/> struct.
    /// </summary>
    /// <param name="x">The X component of the vector.</param>
    /// <param name="y">The Y component of the vector.</param>
    /// <param name="z">The Z component of the vector.</param>
    public Vector3Int(uint x, uint y, uint z)
    {
        this.X = (int)x;
        this.Y = (int)y;
        this.Z = (int)z;
    }

    /// <summary>
    /// Adds two <see cref="Vector3Int"/> instances.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator +(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    /// <summary>
    /// Subtracts the second vector from the first vector.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The difference of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator -(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    /// <summary>
    /// Multiplies two <see cref="Vector3Int"/> instances component-wise.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The component-wise product of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    }

    /// <summary>
    /// Divides the first vector by the second vector component-wise.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The component-wise quotient of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator /(Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.X / b.X, a.Y / b.Y, a.Z / b.Z);
    }

    /// <summary>
    /// Multiplies a <see cref="Vector3Int"/> by a scalar integer value.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The vector scaled by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(Vector3Int a, int b)
    {
        return new Vector3Int(a.X * b, a.Y * b, a.Z * b);
    }

    /// <summary>
    /// Divides a <see cref="Vector3Int"/> by a scalar integer value.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The vector divided by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator /(Vector3Int a, int b)
    {
        return new Vector3Int(a.X / b, a.Y / b, a.Z / b);
    }

    /// <summary>
    /// Multiplies a <see cref="Vector3Int"/> by a scalar unsigned integer value.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The vector scaled by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator *(Vector3Int a, uint b)
    {
        return new Vector3Int(a.X * (int)b, a.Y * (int)b, a.Z * (int)b);
    }

    /// <summary>
    /// Divides a <see cref="Vector3Int"/> by a scalar unsigned integer value.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The vector divided by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3Int operator /(Vector3Int a, uint b)
    {
        return new Vector3Int(a.X / (int)b, a.Y / (int)b, a.Z / (int)b);
    }

    /// <summary>
    /// Determines whether two <see cref="Vector3Int"/> instances are equal.
    /// </summary>
    /// <param name="lhs">The first vector.</param>
    /// <param name="rhs">The second vector.</param>
    /// <returns>true if the vectors are equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector3Int lhs, Vector3Int rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
    }

    /// <summary>
    /// Determines whether two <see cref="Vector3Int"/> instances are not equal.
    /// </summary>
    /// <param name="lhs">The first vector.</param>
    /// <param name="rhs">The second vector.</param>
    /// <returns>true if the vectors are not equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector3Int lhs, Vector3Int rhs)
    {
        return !(lhs == rhs);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Vector3Int"/> to a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="a">The <see cref="Vector3Int"/> to convert.</param>
    /// <returns>A <see cref="Vector3"/> with the same component values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(Vector3Int a)
    {
        return new Vector3(a.X, a.Y, a.Z);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([AllowNull] object obj)
    {
        return obj is Vector3Int vector && this.Equals(vector);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector3Int other)
    {
        return this.X == other.X && this.Y == other.Y && this.Z == other.Z;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return this.X.GetHashCode() ^ this.Y.GetHashCode() << 16 ^ this.Z.GetHashCode() << 24;
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return $"({this.X}, {this.Y}, {this.Z})";
    }

    /// <summary>
    /// Deconstructs the vector into its X, Y, and Z components.
    /// </summary>
    /// <param name="x">The X component of the vector.</param>
    /// <param name="y">The Y component of the vector.</param>
    /// <param name="z">The Z component of the vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out int x, out int y, out int z)
    {
        x = this.X;
        y = this.Y;
        z = this.Z;
    }
}
