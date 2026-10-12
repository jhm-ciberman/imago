using System.Numerics;
using Imago.Rendering.Sprites;

namespace Imago.Gui.Drawing;

/// <summary>
/// This interface provides the functionality to fill a background rectangle.
/// </summary>
public interface IBackground
{
    /// <summary>
    /// Draws a rectangle as a background.
    /// </summary>
    /// <param name="batch">The sprite batch.</param>
    /// <param name="position">The top-left position of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    public void DrawRectangle(SpriteBatch batch, Vector2 position, Vector2 size);
}
