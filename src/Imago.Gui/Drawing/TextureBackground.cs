using System.Numerics;
using Imago.Rendering.Sprites;
using Imago.Support.Drawing;
using Imago.TexturePacking;
using Imago.Textures;

namespace Imago.Gui.Drawing;

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

    /// <inheritdoc/>
    public void DrawRectangle(SpriteBatch batch, Vector2 position, Vector2 size)
    {
        if (this.Texture != null)
        {
            batch.DrawTexture(this.Texture, position, size, this.Color);
        }
    }

    /// <summary>
    /// Allows implicit conversion from a <see cref="Texture"/> to a <see cref="TextureBackground"/>.
    /// </summary>
    /// <param name="texture">The texture to convert.</param>
    /// <returns>A new <see cref="TextureBackground"/> instance with the specified texture.</returns>
    public static implicit operator TextureBackground(Texture texture)
    {
        return new TextureBackground(texture);
    }

    /// <summary>
    /// Allows implicit conversion from a <see cref="PackedTexture"/> to a <see cref="TextureBackground"/>.
    /// </summary>
    /// <param name="texture">The packed texture to convert.</param>
    /// <returns>A new <see cref="TextureBackground"/> instance with the specified packed texture.</returns>
    public static implicit operator TextureBackground(PackedTexture texture)
    {
        return new TextureBackground(texture);
    }
}
