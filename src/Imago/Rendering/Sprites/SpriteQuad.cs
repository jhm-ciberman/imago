using System.Numerics;
using Imago.Support.Drawing;

namespace Imago.Rendering.Sprites;

/// <summary>
/// Represents the four corners of a sprite quad.
/// </summary>
/// <param name="TopLeft">The top-left corner.</param>
/// <param name="TopRight">The top-right corner.</param>
/// <param name="BottomRight">The bottom-right corner.</param>
/// <param name="BottomLeft">The bottom-left corner.</param>
internal readonly record struct SpriteQuad(SpriteVertex TopLeft, SpriteVertex TopRight, SpriteVertex BottomRight, SpriteVertex BottomLeft)
{
    /// <summary>
    /// Creates a quad that covers the given rectangle.
    /// </summary>
    /// <param name="position">The top-left position of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    /// <param name="uvTopLeft">The UV coordinates of the top-left corner.</param>
    /// <param name="uvBottomRight">The UV coordinates of the bottom-right corner.</param>
    /// <param name="color">The color of the quad.</param>
    /// <returns>The quad.</returns>
    public static SpriteQuad FromRectangle(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color)
    {
        return new SpriteQuad(
            new SpriteVertex(position.X, position.Y, 0f, uvTopLeft.X, uvTopLeft.Y, color),
            new SpriteVertex(position.X + size.X, position.Y, 0f, uvBottomRight.X, uvTopLeft.Y, color),
            new SpriteVertex(position.X + size.X, position.Y + size.Y, 0f, uvBottomRight.X, uvBottomRight.Y, color),
            new SpriteVertex(position.X, position.Y + size.Y, 0f, uvTopLeft.X, uvBottomRight.Y, color)
        );
    }

    /// <summary>
    /// Creates a quad that covers the given rectangle, moved by a transformation matrix.
    /// </summary>
    /// <param name="position">The top-left position of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    /// <param name="uvTopLeft">The UV coordinates of the top-left corner.</param>
    /// <param name="uvBottomRight">The UV coordinates of the bottom-right corner.</param>
    /// <param name="transform">The transformation matrix applied to the corners.</param>
    /// <param name="color">The color of the quad.</param>
    /// <returns>The quad.</returns>
    public static SpriteQuad FromRectangle(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color)
    {
        var tl = position;
        var tr = position + new Vector2(size.X, 0f);
        var bl = position + new Vector2(0f, size.Y);
        var br = position + size;

        return new SpriteQuad(
            new SpriteVertex(Vector2.Transform(tl, transform), 0f, uvTopLeft, color),
            new SpriteVertex(Vector2.Transform(tr, transform), 0f, new Vector2(uvBottomRight.X, uvTopLeft.Y), color),
            new SpriteVertex(Vector2.Transform(br, transform), 0f, uvBottomRight, color),
            new SpriteVertex(Vector2.Transform(bl, transform), 0f, new Vector2(uvTopLeft.X, uvBottomRight.Y), color)
        );
    }
}
