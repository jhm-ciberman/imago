using System.Numerics;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Imago.Textures;
using LifeSim.Imago.Materials;
using System.Drawing;

namespace LifeSim.Imago.Controls;

public class TextureBlock : Control
{
    public TextureBlock() : base()
    {
        //
    }

    public TextureBlock(ITextureRegion texture, Vector2 size) : base()
    {
        this.Texture = texture;
        this.Size = size;
    }

    public TextureBlock(ITextureRegion texture)
    {
        this.Texture = texture;
        this.Size = new Vector2(texture.Texture.Width, texture.Texture.Height);
    }

    public TextureBlock(Vector2 size) : base()
    {
        this.Size = size;
    }

    public TextureBlock(Vector2 position, Vector2 size) : base()
    {
        this.Position = position;
        this.Size = size;
    }

    public ITextureRegion? Texture { get; set; }

    public Shader? Shader { get; set; }

    public Vector2 Size { get; set; } = new Vector2(float.NaN, float.NaN);

    public Color Color { get; set; } = Color.White;

    public bool FlipX { get; set; } = false;

    public bool FlipY { get; set; } = false;

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (this.Texture == null)
        {
            if (float.IsNaN(this.Size.X) || float.IsNaN(this.Size.Y))
            {
                return Vector2.Zero;
            }
            else
            {
                return this.Size;
            }
        }

        float width = float.IsNaN(this.Size.X) ? this.Texture.Texture.Width : this.Size.X;
        float height = float.IsNaN(this.Size.Y) ? this.Texture.Texture.Height : this.Size.Y;
        return new Vector2(width, height);
    }

    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);

        if (this.Texture != null)
        {
            var coords = GetTextureCoordinates(this.Texture, this.FlipX, this.FlipY);
            ctx.DrawTexture(this.Shader, this.Texture.Texture, this.Position, this.ActualSize, coords.TopLeft, coords.BottomRight, this.Color);
        }
    }

    private static (Vector2 TopLeft, Vector2 BottomRight) GetTextureCoordinates(ITextureRegion texture, bool flipX, bool flipY)
    {
        Vector2 tl = texture.TopLeft;
        Vector2 br = texture.BottomRight;

        if (flipX) (tl.X, br.X) = (br.X, tl.X);
        if (flipY) (tl.Y, br.Y) = (br.Y, tl.Y);

        return (tl, br);
    }
}
