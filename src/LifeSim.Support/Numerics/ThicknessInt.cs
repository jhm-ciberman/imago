using System;
using System.Globalization;

namespace LifeSim.Support.Numerics;

public struct ThicknessInt : IEquatable<ThicknessInt>
{
    public static ThicknessInt Zero => new ThicknessInt(0);

    public int Left { get; set; }

    public int Top { get; set; }

    public int Right { get; set; }

    public int Bottom { get; set; }

    public Vector2Int TopLeft => new Vector2Int(this.Left, this.Top);

    public Vector2Int TopRight => new Vector2Int(this.Right, this.Top);

    public Vector2Int BottomLeft => new Vector2Int(this.Left, this.Bottom);

    public Vector2Int BottomRight => new Vector2Int(this.Right, this.Bottom);

    public Vector2Int Total => new Vector2Int(this.Left + this.Right, this.Top + this.Bottom);

    public int Horizontal => this.Left + this.Right;

    public int Vertical => this.Top + this.Bottom;

    public ThicknessInt(int all)
    {
        this.Left = all;
        this.Top = all;
        this.Right = all;
        this.Bottom = all;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThicknessInt"/> struct.
    /// </summary>
    /// <param name="horizontal">The horizontal thickness of the frame.</param>
    /// <param name="vertical">The vertical thickness of the frame.</param>
    public ThicknessInt(int horizontal, int vertical)
    {
        this.Left = horizontal;
        this.Top = vertical;
        this.Right = horizontal;
        this.Bottom = vertical;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ThicknessInt"/> struct.
    /// </summary>
    /// <param name="left">The left thickness of the frame.</param>
    /// <param name="top">The top thickness of the frame.</param>
    /// <param name="right">The right thickness of the frame.</param>
    /// <param name="bottom">The bottom thickness of the frame.</param>
    public ThicknessInt(int left, int top, int right, int bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }

    public static ThicknessInt operator +(ThicknessInt a, ThicknessInt b)
    {
        return new ThicknessInt(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);
    }

    public static ThicknessInt operator -(ThicknessInt a, ThicknessInt b)
    {
        return new ThicknessInt(a.Left - b.Left, a.Top - b.Top, a.Right - b.Right, a.Bottom - b.Bottom);
    }

    public static Vector2Int operator +(ThicknessInt a, Vector2Int b)
    {
        return new Vector2Int(a.Left + b.X, a.Top + b.Y);
    }

    public static Vector2Int operator -(ThicknessInt a, Vector2Int b)
    {
        return new Vector2Int(a.Left - b.X, a.Top - b.Y);
    }

    public override string ToString()
    {
        return $"{this.Left}, {this.Top}, {this.Right}, {this.Bottom}";
    }

    public static implicit operator ThicknessInt(int value)
    {
        return new ThicknessInt(value);
    }

    public static implicit operator ThicknessInt(string value)
    {
        var values = value.Split(',');
        var ci = CultureInfo.InvariantCulture;
        return values.Length switch
        {
            1 => new ThicknessInt(int.Parse(values[0], ci)),
            2 => new ThicknessInt(int.Parse(values[0], ci), int.Parse(values[1], ci)),
            4 => new ThicknessInt(int.Parse(values[0], ci), int.Parse(values[1], ci), int.Parse(values[2], ci), int.Parse(values[3], ci)),
            _ => throw new FormatException($"Invalid thickness format. Expected 1, 2 or 4 values, got {values.Length}. Value: {value}"),
        };
    }

    public override bool Equals(object? obj)
    {
        return obj is ThicknessInt thickness && this.Equals(thickness);
    }

    public bool Equals(ThicknessInt other)
    {
        return this.Left == other.Left && this.Top == other.Top && this.Right == other.Right && this.Bottom == other.Bottom;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Left, this.Top, this.Right, this.Bottom);
    }

    public static bool operator ==(ThicknessInt left, ThicknessInt right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ThicknessInt left, ThicknessInt right)
    {
        return !left.Equals(right);
    }
}
