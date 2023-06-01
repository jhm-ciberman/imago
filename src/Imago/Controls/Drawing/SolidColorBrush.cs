using System;
using System.Numerics;
using Imago.Rendering;
using Support;

namespace Imago.Controls.Drawing;

/// <summary>
/// Defines a brush for a solid color.
/// </summary>
public class SolidColorBrush : IBrush, ICloneable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorBrush"/> class.
    /// </summary>
    /// <param name="color">The color of the brush.</param>
    public SolidColorBrush(Color color)
    {
        this.Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorBrush"/> class.
    /// </summary>
    /// <param name="hexColor">The hex color of the brush.</param>
    public SolidColorBrush(string hexColor)
    {
        this.Color = Color.FromHex(hexColor);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SolidColorBrush"/> class.
    /// </summary>
    /// <param name="color">The color of the brush.</param>
    /// <param name="opacity">The opacity of the brush.</param>
    public SolidColorBrush(Color color, float opacity)
    {
        this.Color = color;
        this.Opacity = opacity;
    }

    /// <summary>
    /// Gets or sets the color of the brush.
    /// </summary>
    public Color Color { get; set; } = Color.White;


    /// <summary>
    /// Gets or sets the opacity of the brush.
    /// </summary>
    public float Opacity
    {
        get => this.Color.A / 255.0f;
        set => this.Color = new Color(this.Color.R, this.Color.G, this.Color.B, (byte)(value * 255.0f));
    }

    public void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size)
    {
        if (this.Color.A == 0) return;

        spriteBatcher.DrawRectangle(position, size, this.Color);
    }

    public static implicit operator SolidColorBrush(Color color)
    {
        return new SolidColorBrush(color);
    }

    public virtual object Clone()
    {
        return new SolidColorBrush(this.Color);
    }
}
