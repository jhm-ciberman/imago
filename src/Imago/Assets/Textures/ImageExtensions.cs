using Imago.Support.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Imago.Assets.Textures;

/// <summary>
/// Extension methods for analyzing image pixels.
/// </summary>
public static class ImageExtensions
{
    /// <summary>
    /// Finds the tight bounding rectangle of the visible pixels within a region of the image.
    /// </summary>
    /// <param name="image">The image to scan.</param>
    /// <param name="region">The region to scan in pixels, or null to scan the whole image.</param>
    /// <param name="alphaThreshold">The alpha below which a pixel counts as transparent.</param>
    /// <returns>The tight rectangle in image coordinates, or null when the region is fully transparent.</returns>
    public static RectInt? GetTightRect(this Image<Rgba32> image, RectInt? region = null, byte alphaThreshold = 128)
    {
        var bounds = region ?? new RectInt(0, 0, image.Width, image.Height);

        int left = bounds.Right, right = bounds.Left - 1, top = bounds.Bottom, bottom = bounds.Top - 1;

        for (int y = bounds.Top; y < bounds.Bottom; y++)
        {
            for (int x = bounds.Left; x < bounds.Right; x++)
            {
                if (image[x, y].A < alphaThreshold) continue;

                if (x < left) left = x;
                if (x > right) right = x;
                if (y < top) top = y;
                if (y > bottom) bottom = y;
            }
        }

        if (right < bounds.Left) return null;

        return new RectInt(left, top, right - left + 1, bottom - top + 1);
    }
}
