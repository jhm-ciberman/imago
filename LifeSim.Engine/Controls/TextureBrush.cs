using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

/// <summary>
/// Defines a brush for a texture.
/// </summary>
public class TextureBrush : IBrush
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBrush"/> class.
    /// </summary>
    /// <param name="texture">The texture of the brush.</param>
    public TextureBrush(Texture texture)
    {
        this.Texture = texture;
    }

    /// <summary>
    /// Gets or sets the texture of the brush.
    /// </summary>
    public Texture? Texture { get; set; } = null;

    public void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size)
    {
        if (this.Texture != null)
        {
            spriteBatcher.DrawTexture(null, this.Texture, position, size);
        }
    }

    public static implicit operator TextureBrush(Texture texture)
    {
        return new TextureBrush(texture);
    }
}
