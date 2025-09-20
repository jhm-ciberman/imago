using System.Numerics;
using LifeSim.Imago.Rendering.Sprites;

namespace LifeSim.Imago.Controls.Drawing;

/// <summary>
/// This interface provides the functionality to fill a background rectangle.
/// </summary>
public interface IBackground
{
    /// <summary>
    /// Draws a rectangle as a background.
    /// </summary>
    /// <param name="ctx">The drawing context.</param>
    /// <param name="position">The top-left position of the rectangle.</param>
    /// <param name="size">The size of the rectangle.</param>
    public void DrawRectangle(DrawingContext ctx, Vector2 position, Vector2 size);
}
