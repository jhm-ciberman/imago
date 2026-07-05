using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Imago.Assets.Textures;

/// <summary>
/// Represents a texture created from an image.
/// </summary>
public class ImageTexture : Texture
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageTexture"/> class from an existing image.
    /// The pixel data is copied, so the image remains owned by the caller.
    /// </summary>
    /// <param name="image">The image to create the texture from.</param>
    /// <param name="srgb">Whether to treat the image data as sRGB color space.</param>
    public ImageTexture(Image<Rgba32> image, bool srgb = true)
        : base((uint)image.Width, (uint)image.Height, 0, srgb)
    {
        this.SetDataFromImage(image);
    }

    /// <summary>
    /// Loads an image texture from an image file.
    /// </summary>
    /// <param name="path">The path to the image file to load.</param>
    /// <param name="srgb">Whether to treat the image data as sRGB color space.</param>
    /// <returns>The loaded texture.</returns>
    public static ImageTexture Load(string path, bool srgb = true)
    {
        using var image = Image.Load<Rgba32>(path);
        return new ImageTexture(image, srgb);
    }
}
