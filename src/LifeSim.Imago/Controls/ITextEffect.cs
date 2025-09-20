using System.Numerics;
using FontStashSharp;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a visual effect that can be applied to text.
/// </summary>
public interface ITextEffect
{
    /// <summary>
    /// Draws the specified text with the applied effect.
    /// </summary>
    /// <param name="ctx">The drawing context.</param>
    /// <param name="text">The text to draw.</param>
    /// <param name="font">The font to use for drawing the text.</param>
    /// <param name="position">The position to draw the text.</param>
    /// <param name="color">The color of the text.</param>
    public void Draw(DrawingContext ctx, string text, SpriteFontBase font, Vector2 position, Color color);
}
