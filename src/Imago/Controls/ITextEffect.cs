using System;
using System.Numerics;
using Imago.Graphics;
using Imago.Graphics.Sprites;
using Support;

namespace Imago.Controls;

public interface ITextEffect
{
    /// <summary>
    /// Draws the specified text using the specified color and position with this text effect.
    /// </summary>
    /// <param name="spriteBatcher">The sprite batch to be used.</param>
    /// <param name="text">The text to be drawn.</param>
    /// <param name="font">The font used to draw.</param>
    /// <param name="position">The position of the text.</param>
    /// <param name="color">The color of the text.</param>
    /// <remarks>The <see cref="InvalidateFont"/> method must be called before this method is called.</remarks>
    /// <exception cref="InvalidOperationException">The font is not initialized.</exception>
    void Draw(SpriteBatcher spriteBatcher, string text, Font font, Vector2 position, Color color);
}
