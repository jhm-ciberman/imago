using System;
using System.Numerics;

namespace LifeSim.Support.Numerics;

/// <summary>
/// Represents a 2D rectangle with floating point coordinates.
/// </summary>
public struct Rect : IEquatable<Rect>
{
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
    public void Expand(Vector2 point)
    {
        if (point.X < this.X)
        {
            this.Width += this.X - point.X;
            this.X = point.X;
        }
        else if (point.X > this.Right)
        {
            this.Width = point.X - this.X;
        }

        if (point.Y < this.Y)
        {
            this.Height += this.Y - point.Y;
            this.Y = point.Y;
        }
        else if (point.Y > this.Bottom)
        {
            this.Height = point.Y - this.Y;
        }
    }

    /// <summary>
    /// Expands the rectangle in order to contain the specified rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to contain.</param>
    public void Expand(Rect rect)
    {
        if (rect.X < this.X)
        {
            this.Width += this.X - rect.X;
            this.X = rect.X;
        }
        else if (rect.Right > this.Right)
        {
            this.Width = rect.Right - this.X;
        }

        if (rect.Y < this.Y)
        {
            this.Height += this.Y - rect.Y;
            this.Y = rect.Y;
        }
        else if (rect.Bottom > this.Bottom)
        {
            this.Height = rect.Bottom - this.Y;
        }
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

    public override bool Equals(object? obj)
    {
        return obj is Rect other && this.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.X, this.Y, this.Width, this.Height);
    }

    public override string ToString()
    {
        return $"{this.X}, {this.Y}, {this.Width}, {this.Height}";
    }

    public static bool operator ==(Rect left, Rect right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Rect left, Rect right)
    {
        return !(left == right);
    }

    public static Rect operator *(Rect rect, float scale)
    {
        return new Rect(rect.X * scale, rect.Y * scale, rect.Width * scale, rect.Height * scale);
    }

    public static Rect operator /(Rect rect, float scale)
    {
        return new Rect(rect.X / scale, rect.Y / scale, rect.Width / scale, rect.Height / scale);
    }
}
