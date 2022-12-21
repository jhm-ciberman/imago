using System;
using LifeSim.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LifeSim.Engine.Resources;

public class TextureDrawOperation : IDrawOperation, IDisposable
{
    /// <inheritdoc />
    public Vector2Int Size { get; }

    private readonly Image<Rgba32> _image;

    /// <summary>
    /// Creates a new <see cref="TextureDrawOperation"/> from the given <see cref="Image{Rgba32}"/>.
    /// </summary>
    /// <param name="image">The image to draw.</param>
    public TextureDrawOperation(Image<Rgba32> image)
    {
        this._image = image;
        this.Size = new Vector2Int(image.Width, image.Height);
    }

    /// <summary>
    /// Creates a new <see cref="TextureDrawOperation"/> from an image loaded from disk.
    /// </summary>
    /// <param name="path">The path to the image to load.</param>
    public TextureDrawOperation(string path) : this(Image.Load<Rgba32>(path))
    {
    }

    /// <inheritdoc />
    void IDrawOperation.Draw(Image<Rgba32> destination, Vector2Int position)
    {
        destination.Mutate(ctx => ctx.DrawImage(this._image, new Point(position.X, position.Y), 1f));
    }

    public void Dispose()
    {
        this._image.Dispose();
    }
}