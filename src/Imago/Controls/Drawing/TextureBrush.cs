using System.Numerics;
using Imago.Rendering;
using Imago.TexturePacking;
using Support;

namespace Imago.Controls.Drawing;

/// <summary>
/// Defines a brush for a texture.
/// </summary>
public class TextureBrush : IBrush
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBrush"/> class.
    /// </summary>
    /// <param name="texture">The texture of the brush.</param>
    public TextureBrush(ITextureRegion texture)
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

    public void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size)
    {
        if (this.Texture != null)
        {
            spriteBatcher.DrawTexture(null, this.Texture.Texture, position, size, this.Texture.TopLeft, this.Texture.BottomRight, this.Color);
        }
    }

    public static implicit operator TextureBrush(Texture texture)
    {
        return new TextureBrush(texture);
    }

    public static implicit operator TextureBrush(PackedTexture texture)
    {
        return new TextureBrush(texture);
    }
}
