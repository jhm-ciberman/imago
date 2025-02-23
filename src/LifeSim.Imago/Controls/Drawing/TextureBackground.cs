using System.Numerics;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Imago.TexturePacking;
using LifeSim.Imago.Textures;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls.Drawing;

/// <summary>
/// Defines a brush for a texture.
/// </summary>
public class TextureBackground : IBackground
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBackground"/> class.
    /// </summary>
    /// <param name="texture">The texture of the brush.</param>
    public TextureBackground(ITextureRegion texture)
    {
        this.Texture = texture;
    }

    /// <summary>
    /// Gets or sets the texture of the brush.
    /// </summary>
    public ITextureRegion? Texture { get; set; } = null;

    /// <summary>
    /// Gets or sets the tint color of the brush.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    public void DrawRectangle(DrawingContext ctx, Vector2 position, Vector2 size)
    {
        if (this.Texture != null)
        {
            ctx.DrawTexture(null, this.Texture.Texture, position, size, this.Texture.TopLeft, this.Texture.BottomRight, this.Color);
        }
    }

    public static implicit operator TextureBackground(Texture texture)
    {
        return new TextureBackground(texture);
    }

    public static implicit operator TextureBackground(PackedTexture texture)
    {
        return new TextureBackground(texture);
    }
}
