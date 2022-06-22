using System;
using System.Numerics;
using FontStashSharp;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class ShadowTextEffect : ITextEffect
{
    /// <summary>
    /// Gets or sets the color of the shadow.
    /// </summary>
    public Color Color { get; set; } = LifeSim.Color.Black;

    /// <summary>
    /// Gets or sets the offset of the shadow.
    /// </summary>
    public Vector2 Offset { get; set; } = Vector2.Zero;

    private int _blurAmount = 0;

    /// <summary>
    /// Gets or sets the blur amount of the shadow.
    /// </summary>
    public int BlurAmount
    {
        get => this._blurAmount;
        set
        {
            if (this._blurAmount != value)
            {
                this._blurAmount = value;
                this.InvalidateFontPrivate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the shadow opacity.
    /// </summary>
    public float Opacity
    {
        get => this.Color.A / 255f;
        set => this.Color = new Color(this.Color.R, this.Color.G, this.Color.B, (byte)(value * 255));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowTextEffect"/> class.
    /// </summary>
    /// <param name="color">The color of the shadow.</param>
    /// <param name="offset">The offset of the shadow.</param>
    /// <param name="blurAmount">The blur amount of the shadow.</param>
    public ShadowTextEffect(Color color, Vector2 offset, int blurAmount = 0)
    {
        this.Color = color;
        this.Offset = offset;
        this.BlurAmount = blurAmount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowTextEffect"/> class.
    /// </summary>
    public ShadowTextEffect()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShadowTextEffect"/> class.
    /// </summary>
    /// <param name="color">The color of the shadow.</param>
    /// <param name="offsetX">The offset of the shadow on the x-axis.</param>
    /// <param name="offsetY">The offset of the shadow on the y-axis.</param>
    /// <param name="blurAmount">The blur radius of the shadow.</param>
    public ShadowTextEffect(Color color, float offsetX, float offsetY, int blurAmount = 0)
    {
        this.Color = color;
        this.Offset = new Vector2(offsetX, offsetY);
        this.BlurAmount = blurAmount;
    }

    private SpriteFontBase? _shadowFont = null;

    private SpriteFontBase? _textFont = null;

    private string? _ownerFontFamily = null;

    private int _ownerFontSize = -1;

    public SpriteFontBase InvalidateFont(string? fontFamily, int fontSize)
    {
        this._ownerFontFamily = fontFamily;
        this._ownerFontSize = fontSize;
        this._textFont = FontManager.GetFont(fontFamily, fontSize);
        this._shadowFont = this.BlurAmount == 0 ? this._textFont : FontManager.GetBlurredFont(fontFamily, fontSize, this.BlurAmount);
        return this._textFont;
    }

    private void InvalidateFontPrivate()
    {
        if (this._ownerFontSize == -1) return; // The font hasn't been set yet.
        this.InvalidateFont(this._ownerFontFamily, this._ownerFontSize);
    }

    protected static void ThrowFontNotInitialized()
    {
        throw new InvalidOperationException("Font not initialized. You must call InvalidateFont before calling this method.");
    }

    public void Draw(SpriteBatcher spriteBatcher, string text, Vector2 position, Color color)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        if (this._shadowFont == null)
        {
            ThrowFontNotInitialized();
        }

        // Draw the shadow
        if (this.Color.A > 0)
        {
            var offset = this.Offset - new Vector2(this.BlurAmount, this.BlurAmount);
            spriteBatcher.DrawText(this._shadowFont!, text, position + offset, this.Color);
        }

        // Draw the text
        if (color.A > 0)
        {
            spriteBatcher.DrawText(this._textFont!, text, position, color);
        }
    }
}