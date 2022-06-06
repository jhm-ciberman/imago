using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Controls;

/// <summary>
/// A control that can display a sprite. The class offers functionality for controling the playback of the sprite.
/// </summary>
public class SpriteBlock : Control
{
    private Sprite? _sprite = null;

    private float _frameIndex = 0;

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
    /// Gets or sets the current frame index.
    /// </summary>
    public int FrameIndex
    {
        get => (int)this._frameIndex;
        set
        {
            if (this._frameIndex != value)
            {
                this.UpdateFrame((int)this._frameIndex);
            }
        }
    }

    /// <summary>
    /// Gets or sets the speed of the animation measured in frames per second.
    /// </summary>
    public float FramesPerSecond { get; set; } = 0f;

    /// <summary>
    /// Gets or sets whether the animation should loop.
    /// </summary>
    public bool Loop { get; set; } = true;

    private void UpdateFrame(int frameIndex)
    {
        if (this.Sprite != null)
        {
            frameIndex %= this.Sprite.Frames.Count;
            this._frameIndex = frameIndex;
        }
        else
        {
            this._frameIndex = 0;
        }
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        if (this.Sprite != null)
        {
            var frame = this.Sprite.Frames[(int)this._frameIndex];
            spriteBatcher.DrawTexture(null, frame.Texture, this.Position, this.ActualSize, frame.TopLeft, frame.BottomRight, Color.White);
        }
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        if (this.Sprite != null)
        {
            var frame = this.Sprite.Frames[(int)this._frameIndex];
            return frame.PixelSize;
        }

        return Vector2.Zero;
    }

    protected override Rect ArrangeCore(Rect finalRect)
    {
        var size = this.DesiredSize - this.Margin.Total;
        return new Rect(finalRect.Position, Vector2.Min(size, finalRect.Size));
    }

    public override void Update(float deltaTime)
    {
        if (this.Sprite != null && this.FramesPerSecond != 0)
        {
            this._frameIndex += this.FramesPerSecond * deltaTime;

            if (this.Loop)
            {
                this._frameIndex %= this.Sprite.Frames.Count;
            }
            else
            {
                this._frameIndex = MathUtils.Clamp(this._frameIndex, 0, this.Sprite.Frames.Count - 1);
            }
        }

        base.Update(deltaTime);
    }
}
