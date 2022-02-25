using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LifeSim;

[StructLayout(LayoutKind.Sequential)]
public readonly struct Color
{
    public readonly byte R;
    public readonly byte G;
    public readonly byte B;
    public readonly byte A;

    public Color(uint packed) : this()
    {
        this.R = (byte)(packed >> 0);
        this.G = (byte)(packed >> 8);
        this.B = (byte)(packed >> 16);
        this.A = (byte)(packed >> 24);
    }

    public Color(byte r, byte g, byte b, byte a = 255)
    {
        this.R = r;
        this.G = g;
        this.B = b;
        this.A = a;
    }

    public Color(string hexColor)
    {
        if (hexColor.Length is not 6 and not 8)
        {
            throw new ArgumentException("Invalid hex color string.");
        }

        this.R = byte.Parse(hexColor.AsSpan(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        this.G = byte.Parse(hexColor.AsSpan(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        this.B = byte.Parse(hexColor.AsSpan(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        this.A = hexColor.Length == 8 ? byte.Parse(hexColor.AsSpan(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) : (byte)255;
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
}