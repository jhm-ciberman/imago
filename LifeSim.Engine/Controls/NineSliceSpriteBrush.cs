using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;
using LifeSim.Utils;

namespace LifeSim.Engine.Controls;

public class NineSliceSpriteBrush : SpriteBrush
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NineSliceSpriteBrush"/> class.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    /// <param name="frameIndex">The frame index of the sprite.</param>
    /// <param name="margin">The margin of the sprite for the nine slice mode.</param>
    public NineSliceSpriteBrush(Sprite sprite, int frameIndex, Thickness margin)
        : base(sprite)
    {
        this.FrameIndex = frameIndex;
        this.Margin = margin;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NineSliceSpriteBrush"/> class.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    /// <param name="margin">The margin of the sprite for the nine slice mode.</param>
    public NineSliceSpriteBrush(Sprite sprite, Thickness margin)
        : this(sprite, 0, margin)
    {
    }

    /// <summary>
    /// Gets or sets the margin used when drawing the sprite in 9 slice mode.
    /// </summary>
    public Thickness Margin { get; set; } = Thickness.Zero;

    /// <summary>
    /// Gets or sets whether the center patch of the sprite should be drawn.
    /// </summary>
    public bool DrawCenter { get; set; } = true;

    /// <summary>
    /// Gets or sets the scale used when drawing the sprite in 9 slice mode.
    /// </summary>
    public float Scale { get; set; } = 1f;

    public override void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size)
    {
        var frame = this.GetCurrentTexture();
        if (frame != null)
        {
            spriteBatcher.DrawNinePatch(null, frame, position, size, this.Margin, this.Color, this.DrawCenter, this.Scale);
        }
    }
}