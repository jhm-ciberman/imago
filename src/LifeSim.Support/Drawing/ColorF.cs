using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Support.Drawing;

/// <summary>
/// Represents a color using floating-point RGBA components (0.0 to 1.0).
/// </summary>
public readonly struct ColorF
{
    /// <summary>
    /// Gets the red component of the color.
    /// </summary>
    public readonly float R;

    /// <summary>
    /// Gets the green component of the color.
    /// </summary>
    public readonly float G;

    /// <summary>
    /// Gets the blue component of the color.
    /// </summary>
    public readonly float B;

    /// <summary>
    /// Gets the alpha component of the color.
    /// </summary>
    public readonly float A;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorF"/> struct.
    /// </summary>
    /// <param name="r">The red component (0.0 to 1.0).</param>
    /// <param name="g">The green component (0.0 to 1.0).</param>
    /// <param name="b">The blue component (0.0 to 1.0).</param>
    /// <param name="a">The alpha component (0.0 to 1.0).</param>
    public ColorF(float r, float g, float b, float a = 1f)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorF"/> struct from a hex color string.
    /// </summary>
    /// <param name="hexColor">The hex color string (e.g., "#FF0000" for red).</param>
    public ColorF(string hexColor)
    {
        Color colorTmp = new Color(hexColor);

        this.R = colorTmp.R / 255f;
        this.G = colorTmp.G / 255f;
        this.B = colorTmp.B / 255f;
        this.A = colorTmp.A / 255f;
    }

    /// <summary>
    /// Gets the color white.
    /// </summary>
    public static ColorF White => new ColorF(1f, 1f, 1f, 1f);

    /// <summary>
    /// Gets the color black.
    /// </summary>
    public static ColorF Black => new ColorF(0f, 0f, 0f, 1f);

    /// <summary>
    /// Implicitly converts a <see cref="ColorF"/> to a <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A <see cref="Color"/> with byte component values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(ColorF color)
    {
        return new Color((byte)(color.R * 255f), (byte)(color.G * 255f), (byte)(color.B * 255f), (byte)(color.A * 255f));
    }

    /// <summary>
    /// Implicitly converts a <see cref="ColorF"/> to a <see cref="Vector4"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A <see cref="Vector4"/> with components (R, G, B, A).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector4(ColorF color)
    {
        return new Vector4(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Vector4"/> to a <see cref="ColorF"/>.
    /// </summary>
    /// <param name="color">The vector to convert.</param>
    /// <returns>A <see cref="ColorF"/> with components (X=R, Y=G, Z=B, W=A).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ColorF(Vector4 color)
    {
        return new ColorF(color.X, color.Y, color.Z, color.W);
    }

    /// <summary>
    /// Implicitly converts a <see cref="ColorF"/> to a <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A <see cref="System.Drawing.Color"/> with byte component values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Drawing.Color(ColorF color)
    {
        return System.Drawing.Color.FromArgb((byte)(color.A * 255f), (byte)(color.R * 255f), (byte)(color.G * 255f), (byte)(color.B * 255f));
    }

    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    /// <param name="startColor">The start color.</param>
    /// <param name="endColor">The end color.</param>
    /// <param name="t">The interpolation factor (0.0 to 1.0).</param>
    /// <returns>The interpolated color.</returns>
    public static ColorF Lerp(ColorF startColor, ColorF endColor, float t)
    {
        return new ColorF(
            startColor.R + (endColor.R - startColor.R) * t,
            startColor.G + (endColor.G - startColor.G) * t,
            startColor.B + (endColor.B - startColor.B) * t,
            startColor.A + (endColor.A - startColor.A) * t);
    }

    /// <summary>
    /// Determines whether two <see cref="ColorF"/> instances are equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns>true if the colors are equal; otherwise, false.</returns>
    public static bool Equals(ColorF left, ColorF right)
    {
        return left.R == right.R && left.G == right.G && left.B == right.B && left.A == right.A;
    }

    public override bool Equals(object? obj)
    {
        return obj is ColorF color && Equals(this, color);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.R, this.G, this.B, this.A);
    }

    public override string ToString()
    {
        return $"R: {this.R}, G: {this.G}, B: {this.B}, A: {this.A}";
    }

    /// <summary>
    /// Returns a new color with the specified alpha value.
    /// </summary>
    /// <param name="alpha">The alpha value.</param>
    /// <returns>The new color.</returns>
    public ColorF WithAlpha(float alpha)
    {
        return new ColorF(this.R, this.G, this.B, alpha);
    }

    /// <summary>
    /// Determines whether two <see cref="ColorF"/> instances are equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns>true if the colors are equal; otherwise, false.</returns>
    public static bool operator ==(ColorF left, ColorF right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two <see cref="ColorF"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns>true if the colors are not equal; otherwise, false.</returns>
    public static bool operator !=(ColorF left, ColorF right)
    {
        return !Equals(left, right);
    }
}
