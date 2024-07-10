using System;
using System.Numerics;
using LifeSim.Imago.Graphics.Rendering.Sprites;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls.Drawing;

/// <summary>
/// Defines a brush for a solid color.
/// </summary>
public class ColorBackground : IBackground, ICloneable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ColorBackground"/> class.
    /// </summary>
    /// <param name="color">The color of the brush.</param>
    public ColorBackground(Color color)
    {
        this.Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorBackground"/> class.
    /// </summary>
    /// <param name="hexColor">The hex color of the brush.</param>
    public ColorBackground(string hexColor)
    {
        this.Color = Color.FromHex(hexColor);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorBackground"/> class.
    /// </summary>
    /// <param name="color">The color of the brush.</param>
    /// <param name="opacity">The opacity of the brush.</param>
    public ColorBackground(Color color, float opacity)
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

    public void DrawRectangle(DrawingContext ctx, Vector2 position, Vector2 size)
    {
        if (this.Color.A == 0) return;

        ctx.DrawRectangle(position, size, this.Color);
    }

    public static implicit operator ColorBackground(Color color)
    {
        return new ColorBackground(color);
    }

    public virtual object Clone()
    {
        return new ColorBackground(this.Color);
    }
}
