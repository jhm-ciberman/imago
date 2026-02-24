using System;
using System.Numerics;

namespace Imago.Support.Numerics;

/// <summary>
/// Provides methods to fit rectangles within a container.
/// </summary>
public static class RectFit
{
    /// <summary>
    /// Fits the content rectangle within the container rectangle, maintaining aspect ratio.
    /// The content rectangle will be scaled down to fit within the container rectangle.
    /// </summary>
    /// <param name="container">The container rectangle.</param>
    /// <param name="contentSize">The content size to fit within the container.</param>
    /// <returns>A new rectangle that represents the content rectangle fitted within the container rectangle.</returns>
    public static Rect Contain(Rect container, Vector2 contentSize)
    {
        float scale = Math.Min(container.Width / contentSize.X, container.Height / contentSize.Y);
        float width = contentSize.X * scale;
        float height = contentSize.Y * scale;
        float x = container.X + (container.Width - width) / 2;
        float y = container.Y + (container.Height - height) / 2;
        return new Rect(x, y, width, height);
    }

    /// <summary>
    /// Fits the content rectangle within the container rectangle, maintaining aspect ratio.
    /// The content rectangle will be scaled down to fit within the container rectangle.
    /// </summary>
    /// <param name="containerSize">The size of the container rectangle.</param>
    /// <param name="contentSize">The content size to fit within the container.</param>
    /// <returns>A new rectangle that represents the content rectangle fitted within the container rectangle.</returns>
    public static Rect Contain(Vector2 containerSize, Vector2 contentSize)
    {
        return Contain(new Rect(0, 0, containerSize.X, containerSize.Y), contentSize);
    }

    /// <summary>
    /// Covers the container rectangle with the content rectangle, maintaining aspect ratio.
    /// The content rectangle will be scaled up to cover the entire container rectangle.
    /// </summary>
    /// <param name="container">The container rectangle.</param>
    /// <param name="contentSize">The content size to cover the container.</param>
    /// <returns>A new rectangle that represents the content rectangle covering the container rectangle.</returns>
    public static Rect Cover(Rect container, Vector2 contentSize)
    {
        float scale = Math.Max(container.Width / contentSize.X, container.Height / contentSize.Y);
        float width = contentSize.X * scale;
        float height = contentSize.Y * scale;
        float x = container.X + (container.Width - width) / 2;
        float y = container.Y + (container.Height - height) / 2;
        return new Rect(x, y, width, height);
    }
}
