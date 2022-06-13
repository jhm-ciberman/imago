using System;
using System.Globalization;
using System.Numerics;

namespace LifeSim.Utils;

public struct Thickness
{
    public static Thickness Zero => new Thickness(0);
    public float Left { get; set; }
    public float Top { get; set; }
    public float Right { get; set; }
    public float Bottom { get; set; }
    public Vector2 TopLeft => new Vector2(this.Left, this.Top);
    public Vector2 TopRight => new Vector2(this.Right, this.Top);
    public Vector2 BottomLeft => new Vector2(this.Left, this.Bottom);
    public Vector2 BottomRight => new Vector2(this.Right, this.Bottom);

    public Vector2 Total => new Vector2(this.Left + this.Right, this.Top + this.Bottom);

    public Thickness(float all)
    {
        this.Left = all;
        this.Top = all;
        this.Right = all;
        this.Bottom = all;
    }

    public Thickness(float horizontal, float vertical)
    {
        this.Left = horizontal;
        this.Top = vertical;
        this.Right = horizontal;
        this.Bottom = vertical;
    }

    public Thickness(float left, float top, float right, float bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }

    public static Thickness operator +(Thickness a, Thickness b)
    {
        return new Thickness(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);
    }

    public static Thickness operator -(Thickness a, Thickness b)
    {
        return new Thickness(a.Left - b.Left, a.Top - b.Top, a.Right - b.Right, a.Bottom - b.Bottom);
    }

    public static Vector2 operator +(Thickness a, Vector2 b)
    {
        return new Vector2(a.Left + b.X, a.Top + b.Y);
    }

    public static Vector2 operator -(Thickness a, Vector2 b)
    {
        return new Vector2(a.Left - b.X, a.Top - b.Y);
    }

    public static implicit operator Vector4(Thickness thickness)
    {
        return new Vector4(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
    }

    public static implicit operator Thickness(Vector4 vector)
    {
        return new Thickness(vector.X, vector.Y, vector.Z, vector.W);
    }

    public override string ToString()
    {
        return $"{this.Left}, {this.Top}, {this.Right}, {this.Bottom}";
    }

    public static implicit operator Thickness(float value)
    {
        return new Thickness(value);
    }

    public static implicit operator Thickness(string value)
    {
        var values = value.Split(',');
        var ci = CultureInfo.InvariantCulture;
        return values.Length switch
        {
            1 => new Thickness(float.Parse(values[0], ci)),
            2 => new Thickness(float.Parse(values[0], ci), float.Parse(values[1], ci)),
            4 => new Thickness(float.Parse(values[0], ci), float.Parse(values[1], ci), float.Parse(values[2], ci), float.Parse(values[3], ci)),
            _ => throw new FormatException($"Invalid thickness format. Expected 1, 2 or 4 values, got {values.Length}. Value: {value}"),
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is Thickness thickness)
        {
            return this.Left == thickness.Left && this.Top == thickness.Top && this.Right == thickness.Right && this.Bottom == thickness.Bottom;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.Left.GetHashCode() ^ this.Top.GetHashCode() ^ this.Right.GetHashCode() ^ this.Bottom.GetHashCode();
    }

    public static bool operator ==(Thickness left, Thickness right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Thickness left, Thickness right)
    {
        return !(left == right);
    }
}