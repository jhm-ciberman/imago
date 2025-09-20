using System;
using System.Globalization;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Describes the thickness of a frame around a rectangle using integer values.
/// </summary>
public struct ThicknessInt : IEquatable<ThicknessInt>
{
    /// <summary>
    /// Gets a <see cref="ThicknessInt"/> with all sides set to zero.
    /// </summary>
    public static ThicknessInt Zero => new ThicknessInt(0);

    /// <summary>
    /// Gets or sets the left thickness of the frame.
    /// </summary>
    public int Left { get; set; }

    /// <summary>
    /// Gets or sets the top thickness of the frame.
    /// </summary>
    public int Top { get; set; }

    /// <summary>
    /// Gets or sets the right thickness of the frame.
    /// </summary>
    public int Right { get; set; }

    /// <summary>
    /// Gets or sets the bottom thickness of the frame.
    /// </summary>
    public int Bottom { get; set; }

    /// <summary>
    /// Gets the top left thickness of the frame as a <see cref="Vector2Int"/>.
    /// </summary>
    public Vector2Int TopLeft => new Vector2Int(this.Left, this.Top);

    /// <summary>
    /// Gets the top right thickness of the frame as a <see cref="Vector2Int"/>.
    /// </summary>
    public Vector2Int TopRight => new Vector2Int(this.Right, this.Top);

    /// <summary>
    /// Gets the bottom left thickness of the frame as a <see cref="Vector2Int"/>.
    /// </summary>
    public Vector2Int BottomLeft => new Vector2Int(this.Left, this.Bottom);

    /// <summary>
    /// Gets the bottom right thickness of the frame as a <see cref="Vector2Int"/>.
    /// </summary>
    public Vector2Int BottomRight => new Vector2Int(this.Right, this.Bottom);

    /// <summary>
    /// Gets the total thickness of the frame as a <see cref="Vector2Int"/>.
    /// </summary>
    public Vector2Int Total => new Vector2Int(this.Left + this.Right, this.Top + this.Bottom);

    /// <summary>
    /// Gets the horizontal thickness of the frame.
    /// </summary>
    public int Horizontal => this.Left + this.Right;

    /// <summary>
    /// Gets the vertical thickness of the frame.
    /// </summary>
    public int Vertical => this.Top + this.Bottom;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThicknessInt"/> struct.
    /// </summary>
    /// <param name="all">The thickness value for all four sides.</param>
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

    /// <summary>
    /// Adds two <see cref="ThicknessInt"/> instances.
    /// </summary>
    /// <param name="a">The first thickness.</param>
    /// <param name="b">The second thickness.</param>
    /// <returns>The sum of the two thickness values.</returns>
    public static ThicknessInt operator +(ThicknessInt a, ThicknessInt b)
    {
        return new ThicknessInt(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);
    }

    /// <summary>
    /// Subtracts the second <see cref="ThicknessInt"/> from the first.
    /// </summary>
    /// <param name="a">The first thickness.</param>
    /// <param name="b">The second thickness.</param>
    /// <returns>The difference of the two thickness values.</returns>
    public static ThicknessInt operator -(ThicknessInt a, ThicknessInt b)
    {
        return new ThicknessInt(a.Left - b.Left, a.Top - b.Top, a.Right - b.Right, a.Bottom - b.Bottom);
    }

    /// <summary>
    /// Adds a <see cref="ThicknessInt"/> and a <see cref="Vector2Int"/>.
    /// </summary>
    /// <param name="a">The thickness.</param>
    /// <param name="b">The vector.</param>
    /// <returns>A vector with the thickness's left and top values added to the vector components.</returns>
    public static Vector2Int operator +(ThicknessInt a, Vector2Int b)
    {
        return new Vector2Int(a.Left + b.X, a.Top + b.Y);
    }

    /// <summary>
    /// Subtracts a <see cref="Vector2Int"/> from a <see cref="ThicknessInt"/>.
    /// </summary>
    /// <param name="a">The thickness.</param>
    /// <param name="b">The vector.</param>
    /// <returns>A vector with the vector components subtracted from the thickness's left and top values.</returns>
    public static Vector2Int operator -(ThicknessInt a, Vector2Int b)
    {
        return new Vector2Int(a.Left - b.X, a.Top - b.Y);
    }

    public override string ToString()
    {
        return $"{this.Left}, {this.Top}, {this.Right}, {this.Bottom}";
    }

    /// <summary>
    /// Implicitly converts an integer value to a <see cref="ThicknessInt"/> with all sides set to the same value.
    /// </summary>
    /// <param name="value">The value for all sides.</param>
    /// <returns>A <see cref="ThicknessInt"/> with all sides set to the specified value.</returns>
    public static implicit operator ThicknessInt(int value)
    {
        return new ThicknessInt(value);
    }

    /// <summary>
    /// Implicitly converts a string to a <see cref="ThicknessInt"/>.
    /// </summary>
    /// <param name="value">A comma-separated string with 1, 2, or 4 values.</param>
    /// <returns>A <see cref="ThicknessInt"/> parsed from the string.</returns>
    /// <exception cref="FormatException">Thrown when the string format is invalid.</exception>
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

    /// <summary>
    /// Determines whether two <see cref="ThicknessInt"/> instances are equal.
    /// </summary>
    /// <param name="left">The first thickness.</param>
    /// <param name="right">The second thickness.</param>
    /// <returns>true if the thickness values are equal; otherwise, false.</returns>
    public static bool operator ==(ThicknessInt left, ThicknessInt right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="ThicknessInt"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first thickness.</param>
    /// <param name="right">The second thickness.</param>
    /// <returns>true if the thickness values are not equal; otherwise, false.</returns>
    public static bool operator !=(ThicknessInt left, ThicknessInt right)
    {
        return !left.Equals(right);
    }
}
