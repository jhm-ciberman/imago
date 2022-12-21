using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// supress IDE0057 (I don't want to use the range operator here)
#pragma warning disable IDE0057

namespace LifeSim.Utils;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Color
{
    /// <summary>
    /// Gets the red component of the color.
    /// </summary>
    public readonly byte R;

    /// <summary>
    /// Gets the green component of the color.
    /// </summary>
    public readonly byte G;

    /// <summary>
    /// Gets the blue component of the color.
    /// </summary>
    public readonly byte B;

    /// <summary>
    /// Gets the alpha component of the color.
    /// </summary>
    public readonly byte A;

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="packed">The packed RGBA value as an unsigned integer.</param>
    public Color(uint packed)
    {
        this.R = (byte)(packed >> 0);
        this.G = (byte)(packed >> 8);
        this.B = (byte)(packed >> 16);
        this.A = (byte)(packed >> 24);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="r">The red component of the color.</param>
    /// <param name="g">The green component of the color.</param>
    /// <param name="b">The blue component of the color.</param>
    /// <param name="a">The alpha component of the color.</param>
    public Color(byte r, byte g, byte b, byte a = 255)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    /// <param name="hexColor">The hex color of the color.</param>
    /// <exception cref="ArgumentException">Thrown when the color string is invalid.</exception>
    public Color(string hexColor)
    {
        ReadOnlySpan<char> span = hexColor.AsSpan();
        if (hexColor.StartsWith("#", true, CultureInfo.InvariantCulture))
        {
            span = span[1..];
        }

        if (span.Length is not 6 and not 8)
        {
            throw new ArgumentException("Invalid hex color string.");
        }

        this.R = byte.Parse(span.Slice(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        this.G = byte.Parse(span.Slice(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        this.B = byte.Parse(span.Slice(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        this.A = span.Length == 8 ? byte.Parse(span.Slice(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) : (byte)255;
    }

    /// <summary>
    /// Creates a new <see cref="Color"/> from the specified hex color string.
    /// </summary>
    /// <param name="hexColor">The hex color string.</param>
    /// <returns>The <see cref="Color"/>.</returns>
    public static Color FromHex(string hexColor)
    {
        return new Color(hexColor);
    }

    public static Color FromColorAlpha(Color color, float alpha)
    {
        alpha = color.A / 255f * alpha;
        return new Color(color.R, color.G, color.B, (byte)(alpha * 255));
    }

    public uint ToPackedUInt()
    {
        return (uint)((this.A << 24) | (this.B << 16) | (this.G << 8) | (this.R << 0));
    }

    public static Color White => new Color(255, 255, 255, 255);
    public static Color Gray => new Color(128, 128, 128, 255);
    public static Color LightGray => new Color(192, 192, 192, 255);
    public static Color DarkGray => new Color(64, 64, 64, 255);
    public static Color CoolGray => new Color(140, 146, 172, 255);
    public static Color Black => new Color(0, 0, 0, 255);
    public static Color Red => new Color(255, 0, 0, 255);
    public static Color Green => new Color(0, 255, 0, 255);
    public static Color Blue => new Color(0, 0, 255, 255);
    public static Color Yellow => new Color(255, 255, 0, 255);
    public static Color Cyan => new Color(0, 255, 255, 255);
    public static Color Magenta => new Color(255, 0, 255, 255);
    public static Color Transparent => new Color(0, 0, 0, 0);
    public static Color Orange => new Color(255, 128, 0, 255);
    public static Color Purple => new Color(128, 0, 128, 255);
    public static Color Brown => new Color(128, 64, 0, 255);
    public static Color Pink => new Color(255, 192, 203, 255);
    public static Color Indigo => new Color(75, 0, 130, 255);
    public static Color Violet => new Color(238, 130, 238, 255);
    public static Color GhostWhite => new Color(248, 248, 255, 255);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ColorF(Color color)
    {
        return new ColorF((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Drawing.Color(Color color)
    {
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(System.Drawing.Color color)
    {
        return new Color(color.R, color.G, color.B, color.A);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Color left, Color right)
    {
        return left.R == right.R && left.G == right.G && left.B == right.B && left.A == right.A;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Color left, Color right)
    {
        return !(left == right);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        return obj is Color color && this == color;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.R, this.G, this.B, this.A);
    }
}