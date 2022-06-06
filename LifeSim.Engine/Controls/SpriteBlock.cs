using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Controls;

/// <summary>
/// A control that can display a sprite. The class offers functionality for controling the playback of the sprite.
/// </summary>
public class SpriteBlock : ItemsControl
{
    private Sprite? _sprite = null;

    private PackedTexture? _frame = null;

    private int _frameIndex = 0;

    /// <summary>
    /// Gets or sets the sprite to display. Changing the sprite will reset the frame index.
    /// </summary>
    public Sprite? Sprite
    {
        get => this._sprite;
        set
        {
            if (this._sprite != value)
            {
                this._sprite = value;
                this.UpdateFrame(0);
            }
        }
    }

    /// <summary>
    /// Gets or sets the index of the frame to display.
    /// </summary>
    public int FrameIndex
    {
        get => this._frameIndex;
        set
        {
            if (this._frameIndex != value)
            {
                this.UpdateFrame(this._frameIndex);
            }
        }
    }

    /// <summary>
    /// Gets or sets the speed of the animation measured in frames per second.
    /// </summary>
    public float AnimationSpeed { get; set; } = 30f;

    private void UpdateFrame(int frameIndex)
    {
        if (this.Sprite != null)
        {
            frameIndex %= this.Sprite.Frames.Count;
            this._frame = this.Sprite.Frames[frameIndex];
            this._frameIndex = frameIndex;
        }
        else
        {
            this._frame = null;
            this._frameIndex = 0;
        }
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        if (this._frame != null)
        {
            spriteBatcher.DrawTexture(null, this._frame.Texture, this.Position, this.ActualSize);
        }
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        if (this._frame != null)
        {
            return this._frame.Size;
        }

        return Vector2.Zero;
    }
}
