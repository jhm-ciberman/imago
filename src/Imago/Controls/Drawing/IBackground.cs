using System.Numerics;
using Imago.Graphics.Sprites;

namespace Imago.Controls.Drawing;

/// <summary>
/// This interface provides the functionality to fill a background rectangle.
/// </summary>
public interface IBackground
{
    void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size);
}
