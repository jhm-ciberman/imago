using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace LifeSim.Support.Drawing;

public readonly struct ColorF
{
    public readonly float R;
    public readonly float G;
    public readonly float B;
    public readonly float A;

    public ColorF(float r, float g, float b, float a = 1f)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    public ColorF(string hexColor)
    {
        Color colorTmp = new Color(hexColor);

        this.R = colorTmp.R / 255f;
        this.G = colorTmp.G / 255f;
        this.B = colorTmp.B / 255f;
        this.A = colorTmp.A / 255f;
    }

    public static ColorF White => new ColorF(1f, 1f, 1f, 1f);
    public static ColorF Black => new ColorF(0f, 0f, 0f, 1f);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(ColorF color)
    {
        return new Color((byte)(color.R * 255f), (byte)(color.G * 255f), (byte)(color.B * 255f), (byte)(color.A * 255f));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector4(ColorF color)
    {
        return new Vector4(color.R, color.G, color.B, color.A);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ColorF(Vector4 color)
    {
        return new ColorF(color.X, color.Y, color.Z, color.W);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Drawing.Color(ColorF color)
    {
        return System.Drawing.Color.FromArgb((byte)(color.A * 255f), (byte)(color.R * 255f), (byte)(color.G * 255f), (byte)(color.B * 255f));
    }

    public static ColorF Lerp(ColorF startColor, ColorF endColor, float t)
    {
        return new ColorF(
            startColor.R + (endColor.R - startColor.R) * t,
            startColor.G + (endColor.G - startColor.G) * t,
            startColor.B + (endColor.B - startColor.B) * t,
            startColor.A + (endColor.A - startColor.A) * t);
    }

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

    public static bool operator ==(ColorF left, ColorF right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ColorF left, ColorF right)
    {
        return !Equals(left, right);
    }
}
