using LifeSim.Support.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Imago.Assets.TexturePacking;

/// <summary>
/// Defines a contract for drawing operations that can be performed on texture atlases.
/// </summary>
public interface IDrawOperation
{
    /// <summary>
    /// Gets the size of the texture to be drawn.
    /// </summary>
    public Vector2Int Size { get; }

    /// <summary>
    /// Draws the texture to the specified image.
    /// </summary>
    /// <param name="destination">The image to draw to.</param>
    /// <param name="position">The position to draw the texture at.</param>
    public void Draw(Image<Rgba32> destination, Vector2Int position);
}
