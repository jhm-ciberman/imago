using System.Numerics;
using Imago.Graphics.Materials;
using Imago.Graphics.Sprites;
using Imago.Graphics.Textures;

namespace Imago.Controls;

public class TextureBlock : Control
{
    public TextureBlock() : base()
    {
        //
    }

    public TextureBlock(ITexture texture, Vector2 size) : base()
    {
        this.Texture = texture;
        this.Size = size;
    }

    public TextureBlock(ITexture texture)
    {
        this.Texture = texture;
        this.Size = new Vector2(texture.Width, texture.Height);
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

    public ITexture? Texture { get; set; }

    public Shader? Shader { get; set; }

    public Vector2 Size { get; set; } = new Vector2(float.NaN, float.NaN);

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

        float width = float.IsNaN(this.Size.X) ? this.Texture.Width : this.Size.X;
        float height = float.IsNaN(this.Size.Y) ? this.Texture.Height : this.Size.Y;
        return new Vector2(width, height);
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        if (this.Texture != null)
        {
            spriteBatcher.DrawTexture(this.Shader, this.Texture, this.Position, this.ActualSize);
        }
    }
}
