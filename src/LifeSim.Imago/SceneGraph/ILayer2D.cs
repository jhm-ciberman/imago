using System.Numerics;
using LifeSim.Imago.Graphics.Rendering;

namespace LifeSim.Imago.SceneGraph;

public interface ILayer2D
{
    Matrix4x4 ViewProjectionMatrix { get; }
    Viewport Viewport { get; }
    void Draw(DrawingContext ctx);
    void Update(float deltaTime);
}
