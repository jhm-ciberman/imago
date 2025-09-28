using System;
using System.Numerics;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Represents a 2D rectangle with floating point coordinates.
/// </summary>
public struct Rect : IEquatable<Rect>
{
    /// <summary>
    /// Gets an empty rectangle with all properties set to zero.
    /// </summary>
    public static Rect Empty => default;

    /// <summary>
    /// The X position of the rectangle.
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// The Y position of the rectangle.
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// The width of the rectangle.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// The height of the rectangle.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct.
    /// </summary>
    /// <param name="coords">The position of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    public Rect(Vector2 coords, Vector2 size)
    {
        this.X = coords.X;
        this.Y = coords.Y;
        this.Width = size.X;
        this.Height = size.Y;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rect"/> struct.
    /// </summary>
    /// <param name="x">The X position of the rectangle.</param>
    /// <param name="y">The Y position of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public Rect(float x, float y, float width, float height)
    {
        this.X = x;
        this.Y = y;
        this.Width = width;
        this.Height = height;
    }

    /// <summary>
    /// Deflates the rectangle by the specified amount.
    /// </summary>
    /// <param name="padding">The amount to deflate the rectangle.</param>
    /// <returns>The deflated rectangle.</returns>
    public Rect Deflate(Thickness padding)
    {
        return new Rect(this.X + padding.Left, this.Y + padding.Top, this.Width - padding.Horizontal, this.Height - padding.Vertical);
    }

    /// <summary>
    /// Inflates the rectangle by the specified thickness.
    /// </summary>
    /// <param name="thickness">The thickness to expand by.</param>
    public Rect Inflate(Thickness thickness)
    {
        return new Rect(this.X - thickness.Left, this.Y - thickness.Top, this.Width + thickness.Horizontal, this.Height + thickness.Vertical);
    }

    /// <summary>
    /// Gets or sets the position of the rectangle.
    /// </summary>
    public Vector2 Position
    {
        get => new Vector2(this.X, this.Y);
        set
        {
            this.X = value.X;
            this.Y = value.Y;
        }
    }

    /// <summary>
    /// Gets or sets the size of the rectangle.
    /// </summary>
    public Vector2 Size
    {
        get => new Vector2(this.Width, this.Height);
        set
        {
            this.Width = value.X;
            this.Height = value.Y;
        }
    }

    /// <summary>
    /// Get or sets the rightmost position of the rectangle.
    /// </summary>
    public float Right { get => this.X + this.Width; set => this.Width = value - this.X; }

    /// <summary>
    /// Get or sets the bottommost position of the rectangle.
    /// </summary>
    public float Bottom { get => this.Y + this.Height; set => this.Height = value - this.Y; }

    /// <summary>
    /// Get or sets the leftmost position of the rectangle.
    /// </summary>
    public float Left { get => this.X; set => this.X = value; }

    /// <summary>
    /// Get or sets the topmost position of the rectangle.
    /// </summary>
    public float Top { get => this.Y; set => this.Y = value; }

    /// <summary>
    /// Gets whether the rectangle is empty (i.e., has no width or height).
    /// </summary>
    public bool IsEmpty => this.Width <= 0f || this.Height <= 0f;

    /// <summary>
    /// Returns whether the rectangle contains the specified point.
    /// </summary>
    /// <param name="point">The point to test.</param>
    /// <returns>True if the rectangle contains the point, otherwise false.</returns>
    public bool Contains(Vector2 point)
    {
        return point.X >= this.Left
            && point.Y >= this.Top
            && point.X < this.Right
            && point.Y < this.Bottom;
    }

    /// <summary>
    /// Returns whether the rectangle overlaps the specified rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns>True if the rectangle overlaps the other rectangle, otherwise false.</returns>
    public bool Overlaps(Rect other)
    {
        return other.Left < this.Right
            && other.Right > this.Left
            && other.Top < this.Bottom
            && other.Bottom > this.Top;
    }

    /// <summary>
    /// Returns whether the rectangle is equal to the specified rectangle.
    /// </summary>
    /// <param name="other">The rectangle to test.</param>
    /// <returns>True if the rectangle is equal to the other rectangle, otherwise false.</returns>
    public bool Equals(Rect other)
    {
        return this.X == other.X
            && this.Y == other.Y
            && this.Width == other.Width
            && this.Height == other.Height;
    }

    /// <summary>
    /// Expands the rectangle by the specified amount.
    /// </summary>
    /// <param name="point">The amount to expand the rectangle.</param>
    public Rect Expand(Vector2 point)
    {
        Rect result = this;
        if (point.X < result.X)
        {
            result.Width += result.X - point.X;
            result.X = point.X;
        }
        else if (point.X > result.Right)
        {
            result.Width = point.X - result.X;
        }

        if (point.Y < result.Y)
        {
            result.Height += result.Y - point.Y;
            result.Y = point.Y;
        }
        else if (point.Y > result.Bottom)
        {
            result.Height = point.Y - result.Y;
        }
        return result;
    }

    /// <summary>
    /// Expands the rectangle in order to contain the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to contain.</param>
    public Rect Expand(Rect rect)
    {
        Rect result = this;
        if (rect.X < result.X)
        {
            result.Width += result.X - rect.X;
            result.X = rect.X;
        }
        else if (rect.Right > result.Right)
        {
            result.Width = rect.Right - result.X;
        }

        if (rect.Y < result.Y)
        {
            result.Height += result.Y - rect.Y;
            result.Y = rect.Y;
        }
        else if (rect.Bottom > result.Bottom)
        {
            result.Height = rect.Bottom - result.Y;
        }
        return result;
    }

    /// <summary>
    /// Transforms the rectangle by the specified matrix.
    /// </summary>
    /// <param name="transform">The matrix to transform the rectangle by.</param>
    public void Transform(Matrix3x2 transform)
    {
        var topLeft = Vector2.Transform(new Vector2(this.X, this.Y), transform);
        var topRight = Vector2.Transform(new Vector2(this.Right, this.Y), transform);
        var bottomLeft = Vector2.Transform(new Vector2(this.X, this.Bottom), transform);
        var bottomRight = Vector2.Transform(new Vector2(this.Right, this.Bottom), transform);

        this.X = MathF.Min(topLeft.X, MathF.Min(topRight.X, MathF.Min(bottomLeft.X, bottomRight.X)));
        this.Y = MathF.Min(topLeft.Y, MathF.Min(topRight.Y, MathF.Min(bottomLeft.Y, bottomRight.Y)));
        this.Width = MathF.Max(topLeft.X, MathF.Max(topRight.X, MathF.Max(bottomLeft.X, bottomRight.X))) - this.X;
        this.Height = MathF.Max(topLeft.Y, MathF.Max(topRight.Y, MathF.Max(bottomLeft.Y, bottomRight.Y))) - this.Y;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Rect other && this.Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(this.X, this.Y, this.Width, this.Height);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{this.X}, {this.Y}, {this.Width}, {this.Height}";
    }

    /// <summary>
    /// Creates a rectangle that contains both specified rectangles.
    /// </summary>
    /// <param name="rectA">The first rectangle.</param>
    /// <param name="rectB">The second rectangle.</param>
    /// <returns>A rectangle that contains both input rectangles.</returns>
    public static Rect Union(Rect rectA, Rect rectB)
    {
        var x = MathF.Min(rectA.X, rectB.X);
        var y = MathF.Min(rectA.Y, rectB.Y);
        var width = MathF.Max(rectA.Right, rectB.Right) - x;
        var height = MathF.Max(rectA.Bottom, rectB.Bottom) - y;
        return new Rect(x, y, width, height);
    }

    /// <summary>
    /// Determines whether two <see cref="Rect"/> instances are equal.
    /// </summary>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns>true if the rectangles are equal; otherwise, false.</returns>
    public static bool operator ==(Rect left, Rect right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two <see cref="Rect"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first rectangle.</param>
    /// <param name="right">The second rectangle.</param>
    /// <returns>true if the rectangles are not equal; otherwise, false.</returns>
    public static bool operator !=(Rect left, Rect right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Scales a <see cref="Rect"/> by a specified factor.
    /// </summary>
    /// <param name="rect">The rectangle to scale.</param>
    /// <param name="scale">The scaling factor.</param>
    /// <returns>The scaled rectangle.</returns>
    public static Rect operator *(Rect rect, float scale)
    {
        return new Rect(rect.X * scale, rect.Y * scale, rect.Width * scale, rect.Height * scale);
    }

    /// <summary>
    /// Scales a <see cref="Rect"/> by dividing by a specified factor.
    /// </summary>
    /// <param name="rect">The rectangle to scale.</param>
    /// <param name="scale">The scaling factor to divide by.</param>
    /// <returns>The scaled rectangle.</returns>
    public static Rect operator /(Rect rect, float scale)
    {
        return new Rect(rect.X / scale, rect.Y / scale, rect.Width / scale, rect.Height / scale);
    }

    /// <summary>
    /// Implicitly converts a <see cref="RectInt"/> to a <see cref="Rect"/>.
    /// </summary>
    /// <param name="rect">The <see cref="RectInt"/> to convert.</param>
    /// <returns>A <see cref="Rect"/> with the same dimensions.</returns>
    public static implicit operator Rect(RectInt rect)
    {
        return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
    }
}
