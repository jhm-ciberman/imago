using System;
using System.Globalization;
using System.Numerics;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Describes the thickness of a frame around a rectangle.
/// </summary>
public struct Thickness
{
    /// <summary>
    /// Gets a <see cref="Thickness"/> with all sides set to zero.
    /// </summary>
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
    public Thickness(float horizontal = 0f, float vertical = 0f)
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
    public Thickness(float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }

    /// <summary>
    /// Adds two <see cref="Thickness"/> instances.
    /// </summary>
    /// <param name="a">The first thickness.</param>
    /// <param name="b">The second thickness.</param>
    /// <returns>The sum of the two thickness values.</returns>
    public static Thickness operator +(Thickness a, Thickness b)
    {
        return new Thickness(a.Left + b.Left, a.Top + b.Top, a.Right + b.Right, a.Bottom + b.Bottom);
    }

    /// <summary>
    /// Subtracts the second <see cref="Thickness"/> from the first.
    /// </summary>
    /// <param name="a">The first thickness.</param>
    /// <param name="b">The second thickness.</param>
    /// <returns>The difference of the two thickness values.</returns>
    public static Thickness operator -(Thickness a, Thickness b)
    {
        return new Thickness(a.Left - b.Left, a.Top - b.Top, a.Right - b.Right, a.Bottom - b.Bottom);
    }

    /// <summary>
    /// Adds a <see cref="Thickness"/> and a <see cref="Vector2"/>.
    /// </summary>
    /// <param name="a">The thickness.</param>
    /// <param name="b">The vector.</param>
    /// <returns>A vector with the thickness's left and top values added to the vector components.</returns>
    public static Vector2 operator +(Thickness a, Vector2 b)
    {
        return new Vector2(a.Left + b.X, a.Top + b.Y);
    }

    /// <summary>
    /// Subtracts a <see cref="Vector2"/> from a <see cref="Thickness"/>.
    /// </summary>
    /// <param name="a">The thickness.</param>
    /// <param name="b">The vector.</param>
    /// <returns>A vector with the vector components subtracted from the thickness's left and top values.</returns>
    public static Vector2 operator -(Thickness a, Vector2 b)
    {
        return new Vector2(a.Left - b.X, a.Top - b.Y);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Thickness"/> to a <see cref="Vector4"/>.
    /// </summary>
    /// <param name="thickness">The thickness to convert.</param>
    /// <returns>A <see cref="Vector4"/> with components (Left, Top, Right, Bottom).</returns>
    public static implicit operator Vector4(Thickness thickness)
    {
        return new Vector4(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Vector4"/> to a <see cref="Thickness"/>.
    /// </summary>
    /// <param name="vector">The vector to convert.</param>
    /// <returns>A <see cref="Thickness"/> with components (X=Left, Y=Top, Z=Right, W=Bottom).</returns>
    public static implicit operator Thickness(Vector4 vector)
    {
        return new Thickness(vector.X, vector.Y, vector.Z, vector.W);
    }

    public override string ToString()
    {
        return $"{this.Left}, {this.Top}, {this.Right}, {this.Bottom}";
    }

    /// <summary>
    /// Implicitly converts a float value to a <see cref="Thickness"/> with all sides set to the same value.
    /// </summary>
    /// <param name="value">The value for all sides.</param>
    /// <returns>A <see cref="Thickness"/> with all sides set to the specified value.</returns>
    public static implicit operator Thickness(float value)
    {
        return new Thickness(value);
    }

    /// <summary>
    /// Implicitly converts a string to a <see cref="Thickness"/>.
    /// </summary>
    /// <param name="value">A comma-separated string with 1, 2, or 4 values.</param>
    /// <returns>A <see cref="Thickness"/> parsed from the string.</returns>
    /// <exception cref="FormatException">Thrown when the string format is invalid.</exception>
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

    /// <summary>
    /// Determines whether two <see cref="Thickness"/> instances are equal.
    /// </summary>
    /// <param name="left">The first thickness.</param>
    /// <param name="right">The second thickness.</param>
    /// <returns>true if the thickness values are equal; otherwise, false.</returns>
    public static bool operator ==(Thickness left, Thickness right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Thickness"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first thickness.</param>
    /// <param name="right">The second thickness.</param>
    /// <returns>true if the thickness values are not equal; otherwise, false.</returns>
    public static bool operator !=(Thickness left, Thickness right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Creates a <see cref="Thickness"/> with only the left side set.
    /// </summary>
    /// <param name="left">The left thickness value.</param>
    /// <returns>A <see cref="Thickness"/> with only the left side set to the specified value.</returns>
    public static Thickness FromLeft(float left)
    {
        return new Thickness(left, 0, 0, 0);
    }

    /// <summary>
    /// Creates a <see cref="Thickness"/> with only the top side set.
    /// </summary>
    /// <param name="top">The top thickness value.</param>
    /// <returns>A <see cref="Thickness"/> with only the top side set to the specified value.</returns>
    public static Thickness FromTop(float top)
    {
        return new Thickness(0, top, 0, 0);
    }

    /// <summary>
    /// Creates a <see cref="Thickness"/> with only the right side set.
    /// </summary>
    /// <param name="right">The right thickness value.</param>
    /// <returns>A <see cref="Thickness"/> with only the right side set to the specified value.</returns>
    public static Thickness FromRight(float right)
    {
        return new Thickness(0, 0, right, 0);
    }

    /// <summary>
    /// Creates a <see cref="Thickness"/> with only the bottom side set.
    /// </summary>
    /// <param name="bottom">The bottom thickness value.</param>
    /// <returns>A <see cref="Thickness"/> with only the bottom side set to the specified value.</returns>
    public static Thickness FromBottom(float bottom)
    {
        return new Thickness(0, 0, 0, bottom);
    }

    /// <summary>
    /// Creates a <see cref="Thickness"/> with only the horizontal sides (left and right) set.
    /// </summary>
    /// <param name="horizontal">The horizontal thickness value.</param>
    /// <returns>A <see cref="Thickness"/> with left and right sides set to the specified value.</returns>
    public static Thickness FromHorizontal(float horizontal)
    {
        return new Thickness(horizontal, 0, horizontal, 0);
    }

    /// <summary>
    /// Creates a <see cref="Thickness"/> with only the vertical sides (top and bottom) set.
    /// </summary>
    /// <param name="vertical">The vertical thickness value.</param>
    /// <returns>A <see cref="Thickness"/> with top and bottom sides set to the specified value.</returns>
    public static Thickness FromVertical(float vertical)
    {
        return new Thickness(0, vertical, 0, vertical);
    }
}
