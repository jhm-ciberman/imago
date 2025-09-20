using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// supress IDE0057 (I don't want to use the range operator here)
#pragma warning disable IDE0057

namespace LifeSim.Support.Drawing;

/// <summary>
/// Represents a color using 8-bit RGBA components.
/// </summary>
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
        this = FromHex(hexColor);
    }

    /// <summary>
    /// Creates a new <see cref="Color"/> from the specified hex color string.
    /// </summary>
    /// <param name="hexColor">The hex color string.</param>
    /// <returns>The <see cref="Color"/>.</returns>
    public static Color FromHex(string hexColor)
    {
        if (TryParse(hexColor, out Color color))
            return color;

        throw new ArgumentException("Invalid hex color string.");
    }

    /// <summary>
    /// Tries to parse a hex color string into a <see cref="Color"/>.
    /// </summary>
    /// <param name="hexColor">The hex color string to parse.</param>
    /// <param name="color">The parsed color, if successful.</param>
    /// <returns>true if the parsing was successful; otherwise, false.</returns>
    public static bool TryParse(ReadOnlySpan<char> hexColor, out Color color)
    {
        ReadOnlySpan<char> span = hexColor;
        if (hexColor.StartsWith("#"))
            span = span[1..];

        if (span.Length is 6 or 8)
        {
            // parse components
            var ci = CultureInfo.InvariantCulture;
            var hn = NumberStyles.HexNumber;
            if (byte.TryParse(span[0..2], hn, ci, out byte r) &&
                byte.TryParse(span[2..4], hn, ci, out byte g) &&
                byte.TryParse(span[4..6], hn, ci, out byte b))
            {
                byte a = 255;
                if (span.Length == 8)
                {
                    if (!byte.TryParse(span[6..8], hn, ci, out a))
                    {
                        color = default;
                        return false;
                    }
                }

                color = new Color(r, g, b, a);
                return true;
            }

            color = default;
            return false;
        }


        // Short syntax supported, e.g. #FFF or #FFFF
        if (span.Length is 3 or 4)
        {
            var sr = span[0];
            var sg = span[1];
            var sb = span[2];
            var sa = span.Length == 4 ? span[3] : 'F';
            if (TryParse($"#{sr}{sr}{sg}{sg}{sb}{sb}{sa}{sa}", out color)) // Yes it allocates, but whatever
                return true;
        }

        color = default;
        return false;
    }

    /// <summary>
    /// Creates a new <see cref="Color"/> from an existing color with a modified alpha value.
    /// </summary>
    /// <param name="color">The base color.</param>
    /// <param name="alpha">The alpha multiplier (0.0 to 1.0).</param>
    /// <returns>A new color with the modified alpha value.</returns>
    public static Color FromColorAlpha(Color color, float alpha)
    {
        alpha = color.A / 255f * alpha;
        return new Color(color.R, color.G, color.B, (byte)(alpha * 255));
    }

    /// <summary>
    /// Converts the color to a packed unsigned integer representation.
    /// </summary>
    /// <returns>The color as a packed ARGB unsigned integer.</returns>
    public uint ToPackedUInt()
    {
        return (uint)(this.A << 24 | this.B << 16 | this.G << 8 | this.R << 0);
    }

    /// <summary>
    /// Converts the color to a hexadecimal string representation.
    /// </summary>
    /// <returns>A hex string representation of the color.</returns>
    public string ToHex()
    {
        return this.ToString();
    }

    public override string ToString()
    {
        if (this.A == 255)
            return $"#{this.R:X2}{this.G:X2}{this.B:X2}";

        return $"#{this.R:X2}{this.G:X2}{this.B:X2}{this.A:X2}";
    }

    /// <summary>
    /// Gets the color white.
    /// </summary>
    public static Color White => new Color(255, 255, 255, 255);

    /// <summary>
    /// Gets the color gray.
    /// </summary>
    public static Color Gray => new Color(128, 128, 128, 255);

    /// <summary>
    /// Gets the color light gray.
    /// </summary>
    public static Color LightGray => new Color(192, 192, 192, 255);

    /// <summary>
    /// Gets the color dark gray.
    /// </summary>
    public static Color DarkGray => new Color(64, 64, 64, 255);

    /// <summary>
    /// Gets the color cool gray.
    /// </summary>
    public static Color CoolGray => new Color(140, 146, 172, 255);

    /// <summary>
    /// Gets the color black.
    /// </summary>
    public static Color Black => new Color(0, 0, 0, 255);

    /// <summary>
    /// Gets the color red.
    /// </summary>
    public static Color Red => new Color(255, 0, 0, 255);

    /// <summary>
    /// Gets the color green.
    /// </summary>
    public static Color Green => new Color(0, 255, 0, 255);

    /// <summary>
    /// Gets the color blue.
    /// </summary>
    public static Color Blue => new Color(0, 0, 255, 255);

    /// <summary>
    /// Gets the color yellow.
    /// </summary>
    public static Color Yellow => new Color(255, 255, 0, 255);

    /// <summary>
    /// Gets the color cyan.
    /// </summary>
    public static Color Cyan => new Color(0, 255, 255, 255);

    /// <summary>
    /// Gets the color magenta.
    /// </summary>
    public static Color Magenta => new Color(255, 0, 255, 255);

    /// <summary>
    /// Gets the color transparent (black with zero alpha).
    /// </summary>
    public static Color Transparent => new Color(0, 0, 0, 0);

    /// <summary>
    /// Gets the color orange.
    /// </summary>
    public static Color Orange => new Color(255, 128, 0, 255);

    /// <summary>
    /// Gets the color purple.
    /// </summary>
    public static Color Purple => new Color(128, 0, 128, 255);

    /// <summary>
    /// Gets the color brown.
    /// </summary>
    public static Color Brown => new Color(128, 64, 0, 255);

    /// <summary>
    /// Gets the color pink.
    /// </summary>
    public static Color Pink => new Color(255, 192, 203, 255);

    /// <summary>
    /// Gets the color indigo.
    /// </summary>
    public static Color Indigo => new Color(75, 0, 130, 255);

    /// <summary>
    /// Gets the color violet.
    /// </summary>
    public static Color Violet => new Color(238, 130, 238, 255);

    /// <summary>
    /// Gets the color ghost white.
    /// </summary>
    public static Color GhostWhite => new Color(248, 248, 255, 255);

    /// <summary>
    /// Implicitly converts a <see cref="Color"/> to a <see cref="ColorF"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A <see cref="ColorF"/> with normalized component values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ColorF(Color color)
    {
        return new ColorF(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Color"/> to a <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A <see cref="System.Drawing.Color"/> with the same component values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator System.Drawing.Color(Color color)
    {
        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    /// <summary>
    /// Implicitly converts a <see cref="System.Drawing.Color"/> to a <see cref="Color"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A <see cref="Color"/> with the same component values.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Color(System.Drawing.Color color)
    {
        return new Color(color.R, color.G, color.B, color.A);
    }

    /// <summary>
    /// Determines whether two <see cref="Color"/> instances are equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns>true if the colors are equal; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Color left, Color right)
    {
        return left.R == right.R && left.G == right.G && left.B == right.B && left.A == right.A;
    }

    /// <summary>
    /// Determines whether two <see cref="Color"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first color.</param>
    /// <param name="right">The second color.</param>
    /// <returns>true if the colors are not equal; otherwise, false.</returns>
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

    /// <summary>
    /// Linearly interpolates between two colors.
    /// </summary>
    /// <param name="from">The start color.</param>
    /// <param name="to">The end color.</param>
    /// <param name="t">The interpolation factor (0.0 to 1.0).</param>
    /// <returns>The interpolated color.</returns>
    public static Color Lerp(Color from, Color to, float t)
    {
        t = Math.Clamp(t, 0, 1);

        return new Color(
            (byte)(from.R + (to.R - from.R) * t),
            (byte)(from.G + (to.G - from.G) * t),
            (byte)(from.B + (to.B - from.B) * t),
            (byte)(from.A + (to.A - from.A) * t)
        );
    }
}
