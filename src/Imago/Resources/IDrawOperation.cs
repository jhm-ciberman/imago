using Support;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Imago.Resources;

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
    /// <param name="position">The position to draw the texture at.</param>
    void Draw(Image<Rgba32> destination, Vector2Int position);
}
