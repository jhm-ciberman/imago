using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LifeSim.Engine.Resources;

public class TextureDrawOperation : IDrawOperation
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
    void IDrawOperation.Draw(Image<Rgba32> destination, RectInt rect)
    {
        int x = rect.X;
        int y = rect.Y;
        int w = this._image.Width;
        int h = this._image.Height;
        int r = rect.X - w;
        int b = rect.Y - h;

        if (r > 0)
        { // Right 
            this.DrawImage(destination,
                sx: w - 1, sy: 0, sw: 1, sh: h,
                dx: x + w, dy: y, dw: r, dh: h);
        }

        if (b > 0)
        { // Bottom 
            this.DrawImage(destination,
                sx: 0, sy: h - 1, sw: w, sh: 1,
                dx: x, dy: y + h, dw: w, dh: b);
        }

        if (r > 0 && b > 0)
        { // Bottom Right 
            this.DrawImage(destination,
                sx: w - 1, sy: h - 1, sw: 1, sh: 1,
                dx: x + w, dy: y + h, dw: r, dh: b);
        }

        // The actual image:
        destination.Mutate(ctx => ctx.DrawImage(this._image, new Point(x, y), 1f));
    }

    // x = source, d = destination
    private void DrawImage(Image<Rgba32> dst, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
    {
        var src = this._image;
        float scaleX = sw / dw;
        float scaleY = sh / dh;
        for (int xx = 0; xx < dw; xx++)
        {
            for (int yy = 0; yy < dh; yy++)
            {
                int destX = dx + xx;
                int destY = dy + yy;
                if (destX >= 0 && destY >= 0 && destX < dst.Width && destY < dst.Height)
                {
                    var col = src[sx + (int) (xx * scaleX), sy + (int) (yy * scaleY)];
                    dst[destX, destY] = col;
                }
            }
        }
    }
}