using System.Numerics;
using LifeSim.Imago.Rendering.Sprites;

namespace LifeSim.Imago.Controls.Drawing;

/// <summary>
/// This interface provides the functionality to fill a background rectangle.
/// </summary>
public interface IBackground
{
    public void DrawRectangle(DrawingContext ctx, Vector2 position, Vector2 size);
}
