using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Controls;

/// <summary>
/// Defines a brush for an animated sprite.
/// </summary>
public class SpriteBrush : IAnimatedBrush
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
                this.SetFrame(0);
            }
        }
    }

    /// <summary>
    /// Gets or sets the current frame index.
    /// </summary>
    public int FrameIndex
    {
        get => (int)this._frameIndex;
        set => this.SetFrame((int)value);
    }

    /// <summary>
    /// Gets or sets the speed of the animation measured in frames per second.
    /// A negative value means the animation is played backwards.
    /// </summary>
    public float FramesPerSecond { get; set; } = 0f;

    /// <summary>
    /// Gets or sets whether the animation should loop.
    /// </summary>
    public bool Loop { get; set; } = true;


    /// <summary>
    /// Gets or sets the tint color of the sprite.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBrush"/> class.
    /// </summary>
    public SpriteBrush()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBrush"/> class.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    public SpriteBrush(Sprite? sprite)
    {
        this.Sprite = sprite;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBrush"/> class.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    /// <param name="color">The tint color of the sprite.</param>
    public SpriteBrush(Sprite? sprite, Color color)
    {
        this.Sprite = sprite;
        this.Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBrush"/> class.
    /// </summary>
    /// <param name="sprite">The sprite to display.</param>
    /// <param name="fps">The speed of the animation measured in frames per second. A negative value means the animation is played backwards.</param>
    /// <param name="loop">Whether the animation should loop.</param>
    /// <param name="color">The tint color of the sprite.</param>
    /// <param name="frameIndex">The initial frame index.</param>
    public SpriteBrush(Sprite? sprite, float fps, bool loop = true, Color? color = null, int frameIndex = 0)
    {
        this.Sprite = sprite;
        this.FramesPerSecond = fps;
        this.Loop = loop;
        this.Color = color ?? Color.White;
        this.SetFrame(frameIndex);
    }

    private void SetFrame(int frameIndex)
    {
        if (this._frameIndex != frameIndex)
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
    }

    public void Update(float deltaTime)
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
    }

    public void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size)
    {
        if (this.Sprite != null)
        {
            var frame = this.Sprite.Frames[(int)this._frameIndex];
            spriteBatcher.DrawTexture(null, frame.Texture, position, size, frame.TopLeft, frame.BottomRight, this.Color);
        }
    }
}
