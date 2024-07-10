using System.Numerics;
using LifeSim.Imago.Graphics.Rendering.Sprites;

namespace LifeSim.Imago.Controls.Drawing;

/// <summary>
/// This interface provides the functionality to fill a background rectangle.
/// </summary>
public interface IBackground
{
    void DrawRectangle(DrawingContext ctx, Vector2 position, Vector2 size);
}
