using System;
using System.Globalization;
using System.Numerics;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Describes the thickness of a frame around a rectangle.
/// </summary>
public struct Thickness
{
    public static Thickness Zero => new Thickness(0);

    /// <summary>
    /// The left thickness of the frame.
    /// </summary>
    public float Left { get; set; }

    /// <summary>
    /// The top thickness of the frame.
    /// </summary>
    public float Top { get; set; }

    /// <summary>
    /// The right thickness of the frame.
    /// </summary>
    public float Right { get; set; }

    /// <summary>
    /// The bottom thickness of the frame.
    /// </summary>
    public float Bottom { get; set; }

    /// <summary>
    /// The top left thickness of the frame as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 TopLeft => new Vector2(this.Left, this.Top);

    /// <summary>
    /// The top right thickness of the frame as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 TopRight => new Vector2(this.Right, this.Top);

    /// <summary>
    /// The bottom left thickness of the frame as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 BottomLeft => new Vector2(this.Left, this.Bottom);

    /// <summary>
    /// The bottom right thickness of the frame as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 BottomRight => new Vector2(this.Right, this.Bottom);

    /// <summary>
    /// The total thickness of the frame as a <see cref="Vector2"/>.
    /// </summary>
    public Vector2 Total => new Vector2(this.Left + this.Right, this.Top + this.Bottom);

    /// <summary>
    /// The horizontal thickness of the frame.
    /// </summary>
    public float Horizontal => this.Left + this.Right;

    /// <summary>
    /// The vertical thickness of the frame.
    /// </summary>
    public float Vertical => this.Top + this.Bottom;

    /// <summary>
    /// Initializes a new instance of the <see cref="Thickness"/> struct.
    /// </summary>
    /// <param name="all">The thickness of the frame. This value is used for all four sides.</param>
    public Thickness(float all)
    {
        this.Left = all;
        this.Top = all;
        this.Right = all;
        this.Bottom = all;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Thickness"/> struct.
    /// </summary>
    /// <param name="horizontal">The horizontal thickness of the frame that will be used for the left and right sides.</param>
    /// <param name="vertical">The vertical thickness of the frame that will be used for the top and bottom sides.</param>
    public Thickness(float horizontal, float vertical)
    {
        this.Left = horizontal;
        this.Top = vertical;
        this.Right = horizontal;
        this.Bottom = vertical;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Thickness"/> struct.
    /// </summary>
    /// <param name="left">The left thickness of the frame.</param>
    /// <param name="top">The top thickness of the frame.</param>
    /// <param name="right">The right thickness of the frame.</param>
    /// <param name="bottom">The bottom thickness of the frame.</param>
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
            return this.Left == thickness.Left && this.Top == thickness.Top && this.Right == thickness.Right && this.Bottom == thickness.Bottom;

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

    public static Thickness FromLeft(float left)
    {
        return new Thickness(left, 0, 0, 0);
    }

    public static Thickness FromTop(float top)
    {
        return new Thickness(0, top, 0, 0);
    }

    public static Thickness FromRight(float right)
    {
        return new Thickness(0, 0, right, 0);
    }

    public static Thickness FromBottom(float bottom)
    {
        return new Thickness(0, 0, 0, bottom);
    }

    public static Thickness FromHorizontal(float horizontal)
    {
        return new Thickness(horizontal, 0, horizontal, 0);
    }

    public static Thickness FromVertical(float vertical)
    {
        return new Thickness(0, vertical, 0, vertical);
    }
}
