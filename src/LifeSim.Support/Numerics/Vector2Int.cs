using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Represents a 2D vector with integer components.
/// </summary>
public struct Vector2Int : IEquatable<Vector2Int>
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
    /// Gets a vector with both components set to 1.
    /// </summary>
    public static Vector2Int One => new Vector2Int(1, 1);

    /// <summary>
    /// Gets a vector with both components set to 0.
    /// </summary>
    public static Vector2Int Zero => new Vector2Int(0, 0);

    /// <summary>
    /// Gets a vector with the X component set to 1 and the Y component set to 0.
    /// </summary>
    public static Vector2Int UnitX => new Vector2Int(1, 0);

    /// <summary>
    /// Gets a vector with the X component set to 0 and the Y component set to 1.
    /// </summary>
    public static Vector2Int UnitY => new Vector2Int(0, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2Int"/> struct.
    /// </summary>
    /// <param name="x">The X component of the vector.</param>
    /// <param name="y">The Y component of the vector.</param>
    public Vector2Int(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Vector2Int"/> struct.
    /// </summary>
    /// <param name="x">The X component of the vector.</param>
    /// <param name="y">The Y component of the vector.</param>
    public Vector2Int(uint x, uint y)
    {
        this.X = (int)x;
        this.Y = (int)y;
    }

    /// <summary>
    /// Returns a vector whose elements are the maximum of each of the pairs of elements in two specified vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The vector containing the maximum values from both vectors.</returns>
    public static Vector2Int Max(Vector2Int value1, Vector2Int value2)
    {
        return new Vector2Int(Math.Max(value1.X, value2.X), Math.Max(value1.Y, value2.Y));
    }

    /// <summary>
    /// Returns a vector whose elements are the minimum of each of the pairs of elements in two specified vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <returns>The vector containing the minimum values from both vectors.</returns>
    public static Vector2Int Min(Vector2Int value1, Vector2Int value2)
    {
        return new Vector2Int(Math.Min(value1.X, value2.X), Math.Min(value1.Y, value2.Y));
    }

    /// <summary>
    /// Clamps the specified coordinates to the nearest valid coordinates based on the given vector.
    /// </summary>
    /// <param name="value">The vector to clamp.</param>
    /// <param name="min">The minimum vector.</param>
    /// <param name="max">The maximum vector.</param>
    /// <returns>The clamped vector.</returns>
    public static Vector2Int Clamp(Vector2Int value, Vector2Int min, Vector2Int max)
    {
        return new Vector2Int(Math.Clamp(value.X, min.X, max.X), Math.Clamp(value.Y, min.Y, max.Y));
    }

    /// <summary>
    /// Linearly interpolates between two vectors.
    /// </summary>
    /// <param name="value1">The first vector.</param>
    /// <param name="value2">The second vector.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated vector.</returns>
    public static Vector2Int Lerp(Vector2Int value1, Vector2Int value2, float t)
    {
        return new Vector2Int(
            (int)Math.Round(float.Lerp(value1.X, value2.X, t)),
            (int)Math.Round(float.Lerp(value1.Y, value2.Y, t)));
    }

    /// <summary>
    /// Returns the absolute value of the vector for each component.
    /// </summary>
    /// <param name="value">The vector to get the absolute value of.</param>
    /// <returns>The absolute value of the vector.</returns>
    public static Vector2Int Abs(Vector2Int value)
    {
        return new Vector2Int(Math.Abs(value.X), Math.Abs(value.Y));
    }

    /// <summary>
    /// Clamps the vector to the specified bounds.
    /// </summary>
    /// <param name="bounds">The bounds to clamp the vector to.</param>
    /// <returns>The clamped vector.</returns>
    public Vector2Int Clamp(RectInt bounds)
    {
        return new Vector2Int(
            Math.Clamp(this.X, bounds.X, bounds.X + bounds.Width),
            Math.Clamp(this.Y, bounds.Y, bounds.Y + bounds.Height)
        );
    }

    /// <summary>
    /// Adds two <see cref="Vector2Int"/> instances.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The sum of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator +(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X + b.X, a.Y + b.Y);
    }

    /// <summary>
    /// Subtracts the second vector from the first vector.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The difference of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator -(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X - b.X, a.Y - b.Y);
    }

    /// <summary>
    /// Multiplies two <see cref="Vector2Int"/> instances component-wise.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The component-wise product of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X * b.X, a.Y * b.Y);
    }

    /// <summary>
    /// Divides the first vector by the second vector component-wise.
    /// </summary>
    /// <param name="a">The first vector.</param>
    /// <param name="b">The second vector.</param>
    /// <returns>The component-wise quotient of the two vectors.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator /(Vector2Int a, Vector2Int b)
    {
        return new Vector2Int(a.X / b.X, a.Y / b.Y);
    }

    /// <summary>
    /// Multiplies a <see cref="Vector2Int"/> by a scalar integer value.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The vector scaled by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(Vector2Int a, int b)
    {
        return new Vector2Int(a.X * b, a.Y * b);
    }

    /// <summary>
    /// Multiplies a scalar integer value by a <see cref="Vector2Int"/>.
    /// </summary>
    /// <param name="a">The scalar value.</param>
    /// <param name="b">The vector.</param>
    /// <returns>The vector scaled by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(int a, Vector2Int b)
    {
        return new Vector2Int(a * b.X, a * b.Y);
    }

    /// <summary>
    /// Divides a <see cref="Vector2Int"/> by a scalar integer value.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The vector divided by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator /(Vector2Int a, int b)
    {
        return new Vector2Int(a.X / b, a.Y / b);
    }

    /// <summary>
    /// Multiplies a <see cref="Vector2Int"/> by a scalar unsigned integer value.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The vector scaled by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator *(Vector2Int a, uint b)
    {
        return new Vector2Int(a.X * (int)b, a.Y * (int)b);
    }

    /// <summary>
    /// Divides a <see cref="Vector2Int"/> by a scalar unsigned integer value.
    /// </summary>
    /// <param name="a">The vector.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The vector divided by the scalar value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator /(Vector2Int a, uint b)
    {
        return new Vector2Int(a.X / (int)b, a.Y / (int)b);
    }

    /// <summary>
    /// Negates a <see cref="Vector2Int"/>.
    /// </summary>
    /// <param name="a">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2Int operator -(Vector2Int a)
    {
        return new Vector2Int(-a.X, -a.Y);
    }

    /// <summary>
    /// Determines whether two <see cref="Vector2Int"/> instances are equal.
    /// </summary>
    /// <param name="lhs">The first vector.</param>
    /// <param name="rhs">The second vector.</param>
    /// <returns>true if the vectors are equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector2Int lhs, Vector2Int rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y;
    }

    /// <summary>
    /// Determines whether two <see cref="Vector2Int"/> instances are not equal.
    /// </summary>
    /// <param name="lhs">The first vector.</param>
    /// <param name="rhs">The second vector.</param>
    /// <returns>true if the vectors are not equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector2Int lhs, Vector2Int rhs)
    {
        return !(lhs == rhs);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Vector2Int"/> to a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="a">The <see cref="Vector2Int"/> to convert.</param>
    /// <returns>A <see cref="Vector2"/> with the same component values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(Vector2Int a)
    {
        return new Vector2(a.X, a.Y);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals([AllowNull] object obj)
    {
        return obj is Vector2Int vector && this.Equals(vector);
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Vector2Int other)
    {
        return this.X == other.X && this.Y == other.Y;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return this.X.GetHashCode() ^ this.Y.GetHashCode() << 16;
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return "(" + this.X + ", " + this.Y + ")";
    }

    /// <summary>
    /// Deconstructs the vector into its X and Y components.
    /// </summary>
    /// <param name="x">The X component of the vector.</param>
    /// <param name="y">The Y component of the vector.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out int x, out int y)
    {
        x = this.X;
        y = this.Y;
    }
}
