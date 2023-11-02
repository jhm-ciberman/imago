using System.Numerics;
using Imago.Rendering.Sprites;
using Support;

namespace Imago.Controls.Drawing;

/// <summary>
/// Defines a brush for an animated sprite.
/// </summary>
public class SpriteBackground : IBackground
{
    private Sprite _sprite;

    private float _frameIndex = 0;

    /// <summary>
    /// Gets or sets the sprite to display. Changing the sprite will reset the frame index.
    /// </summary>
    public Sprite Sprite
    {
        get => this._sprite;
        set
        {
            if (this._sprite != value)
            {
                this._sprite = value;
                this.FrameIndex = 0;
            }
        }
    }

    /// <summary>
    /// Gets or sets the current frame index.
    /// </summary>
    public int FrameIndex
    {
        get => (int)this._frameIndex;
        set => this._frameIndex = value % this.Sprite.Frames.Count;
    }

    /// <summary>
    /// Gets or sets the tint color of the sprite.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBackground"/> class.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    public SpriteBackground(Sprite sprite, int frameIndex = 0)
    {
        this._sprite = sprite;
        this.FrameIndex = frameIndex;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBackground"/> class.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    /// <param name="color">The tint color of the sprite.</param>
    public SpriteBackground(Sprite sprite, Color color)
    {
        this._sprite = sprite;
        this.Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBackground"/> class.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    /// <param name="color">The tint color of the sprite.</param>
    /// <param name="frameIndex">The initial frame index.</param>
    public SpriteBackground(Sprite sprite, Color color, int frameIndex = 0)
    {
        this._sprite = sprite;
        this.Color = color;
        this.FrameIndex = frameIndex;
    }

    public virtual void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size)
    {
        int frameIndex = (int)this._frameIndex;
        var frame = this.Sprite.Frames[frameIndex];
        var sprite = this.Sprite;
        if (sprite.IsNineSlice)
        {
            spriteBatcher.DrawNinePatch(null, frame, position, size, sprite.NineSliceMargin, this.Color, sprite.Scale);
        }
        else
        {
            spriteBatcher.DrawTexture(null, frame.Texture, position, size, frame.TopLeft, frame.BottomRight, this.Color);
        }
    }

    public static implicit operator SpriteBackground(Sprite sprite)
    {
        return new SpriteBackground(sprite, 0);
    }
}
