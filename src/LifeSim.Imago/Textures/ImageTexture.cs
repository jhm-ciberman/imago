using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Imago.Textures;

public class ImageTexture : Texture
{
    private readonly Image<Rgba32> _image;

    public ImageTexture(Image<Rgba32> image, bool srgb = true)
        : base((uint)image.Width, (uint)image.Height, 0, srgb)
    {
        this._image = image;
        this.SetDataFromImage(image);
    }

    public ImageTexture(string path, bool srgb = true)
        : this(Image.Load<Rgba32>(path), srgb)
    {
    }

    public override void Dispose()
    {
        base.Dispose();

        this._image.Dispose();
    }
}
