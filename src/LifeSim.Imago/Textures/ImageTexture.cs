using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Imago.Textures;

/// <summary>
/// Represents a texture created from an image file or Image instance.
/// </summary>
public class ImageTexture : Texture
{
    private readonly Image<Rgba32> _image;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageTexture"/> class from an existing image.
    /// </summary>
    /// <param name="image">The image to create the texture from.</param>
    /// <param name="srgb">Whether to treat the image data as sRGB color space.</param>
    public ImageTexture(Image<Rgba32> image, bool srgb = true)
        : base((uint)image.Width, (uint)image.Height, 0, srgb)
    {
        this._image = image;
        this.SetDataFromImage(image);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageTexture"/> class from an image file.
    /// </summary>
    /// <param name="path">The path to the image file to load.</param>
    /// <param name="srgb">Whether to treat the image data as sRGB color space.</param>
    public ImageTexture(string path, bool srgb = true)
        : this(Image.Load<Rgba32>(path), srgb)
    {
    }

    /// <summary>
    /// Disposes the texture and releases associated resources.
    /// </summary>
    public override void Dispose()
    {
        base.Dispose();

        this._image.Dispose();
    }
}
