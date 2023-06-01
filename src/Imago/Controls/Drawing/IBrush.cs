using System.Numerics;
using Imago.Rendering;

namespace Imago.Controls.Drawing;

/// <summary>
/// This interface provides the functionality to fill a rectangle with a brush.
/// A brush can be a texture, a color or a sprite.
/// </summary>
public interface IBrush
{
    void DrawRectangle(SpriteBatcher spriteBatcher, Vector2 position, Vector2 size);
}
