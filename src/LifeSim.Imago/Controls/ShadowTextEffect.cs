using System.Numerics;
using LifeSim.Imago.Graphics;
using LifeSim.Imago.Graphics.Rendering.Sprites;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

public class ShadowTextEffect : ITextEffect
{
    /// <summary>
    /// Gets or sets the color of the shadow.
    /// </summary>
    public Color Color { get; set; } = Color.Black;

    /// <summary>
    /// Gets or sets the offset of the shadow.
    /// </summary>
    public Vector2 Offset { get; set; } = Vector2.Zero;

    /// <summary>
    /// Gets or sets the blur amount of the shadow.
    /// </summary>
    public int BlurAmount { get; set; } = 0;

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

    public void Draw(DrawingContext ctx, string text, Font font, Vector2 position, Color color)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        // Draw the shadow
        if (this.Color.A > 0)
        {
            var offset = this.Offset - new Vector2(this.BlurAmount, this.BlurAmount);
            var shadowFont = this.BlurAmount == 0 ? font : Font.GetBlurredFont(font.FontFamily, font.FontSize, this.BlurAmount);
            ctx.DrawText(shadowFont, text, position + offset, this.Color);
        }

        // Draw the text
        if (color.A > 0)
        {
            ctx.DrawText(font, text, position, color);
        }
    }
}
