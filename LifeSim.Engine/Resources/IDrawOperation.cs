using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Engine.Resources;

public interface IDrawOperation
{
    /// <summary>
    /// Gets the size of the texture to be drawn.
    /// </summary>
    Vector2Int Size { get; }

    /// <summary>
    /// Draws the texture to the specified image.
    /// </summary>
    /// <param name="destination">The image to draw to.</param>
    /// <param name="rect">The rectangle to draw to. It's size is at least as big as the texture, but it can be bigger.</param>
    void Draw(Image<Rgba32> destination, RectInt rect);
}