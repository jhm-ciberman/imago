using System;
using System.Numerics;
using LifeSim.Imago.Graphics;
using LifeSim.Imago.Graphics.Rendering;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Controls;

public interface ITextEffect
{
    /// <summary>
    /// Draws the specified text using the specified color and position with this text effect.
    /// </summary>
    /// <param name="ctx">The drawing context to use.</param>
    /// <param name="text">The text to be drawn.</param>
    /// <param name="font">The font used to draw.</param>
    /// <param name="position">The position of the text.</param>
    /// <param name="color">The color of the text.</param>
    /// <exception cref="InvalidOperationException">The font is not initialized.</exception>
    void Draw(DrawingContext ctx, string text, Font font, Vector2 position, Color color);
}
