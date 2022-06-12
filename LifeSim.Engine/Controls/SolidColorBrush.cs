using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

/// <summary>
/// Defines a brush for a solid color.
/// </summary>
public class SolidColorBrush : IBrush
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
    /// Gets or sets the color of the brush.
    /// </summary>
    public Color Color { get; set; } = Color.White;

    public void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size)
    {
        spriteBatcher.DrawRectangle(position, size, this.Color);
    }

    public static implicit operator SolidColorBrush(Color color)
    {
        return new SolidColorBrush(color);
    }
}
