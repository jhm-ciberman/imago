using System.Numerics;
using Imago.Rendering.Sprites;

namespace Imago.SceneGraph;

public interface ILayer2D
{
    Matrix4x4 ViewProjectionMatrix { get; }
    Viewport Viewport { get; }
    void Draw(SpriteBatcher spriteBatcher);
    void Update(float deltaTime);
}
