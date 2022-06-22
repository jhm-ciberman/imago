using System;
using System.Numerics;
using FontStashSharp;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public interface ITextEffect
{
    /// <summary>
    /// Draws the specified text using the specified color and position with this text effect.
    /// </summary>
    /// <param name="spriteBatcher">The sprite batch to be used.</param>
    /// <param name="text">The text to be drawn.</param>
    /// <param name="position">The position of the text.</param>
    /// <param name="color">The color of the text.</param>
    /// <remarks>The <see cref="InvalidateFont"/> method must be called before this method is called.</remarks>
    /// <exception cref="InvalidOperationException">The font is not initialized.</exception>
    void Draw(SpriteBatcher spriteBatcher, string text, Vector2 position, Color color);


    /// <summary>
    /// This method should be called by the owning control before the first draw call
    /// and whenever the font family, font size, or other font properties change.
    /// The method returns the new font that the control should use for size calculations.
    /// </summary>
    /// <param name="fontFamily">The new font family.</param>
    /// <param name="fontSize">The new font size.</param>
    /// <returns>The new font that the control should use for size calculations.</returns>
    SpriteFontBase InvalidateFont(string? fontFamily, int fontSize);
}
