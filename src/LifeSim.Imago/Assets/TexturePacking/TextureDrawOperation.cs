using System;
using LifeSim.Support.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LifeSim.Imago.Assets.TexturePacking;

/// <summary>
/// Represents a drawing operation that can render an image onto a texture atlas.
/// </summary>
public class TextureDrawOperation : IDrawOperation, IDisposable
{
    /// <inheritdoc />
    public Vector2Int Size { get; }

    private readonly Image _image;

    /// <summary>
    /// Creates a new <see cref="TextureDrawOperation"/> from the given <see cref="Image{Rgba32}"/>.
    /// </summary>
    /// <param name="image">The image to draw.</param>
    public TextureDrawOperation(Image image)
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

    private static GraphicsOptions GraphicsOptions { get; } = new GraphicsOptions
    {
        Antialias = false, // We are just copying, we don't need antialiasing
        AlphaCompositionMode = PixelAlphaCompositionMode.Src, // We want to override the alpha channel, not blend it
    };

    /// <inheritdoc />
    void IDrawOperation.Draw(Image<Rgba32> destination, Vector2Int position)
    {
        destination.Mutate(ctx => ctx.DrawImage(
            foreground: this._image,
            backgroundLocation: new Point(position.X, position.Y),
            options: GraphicsOptions)
        );
    }

    /// <summary>
    /// Disposes the texture draw operation and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        this._image.Dispose();
    }
}
