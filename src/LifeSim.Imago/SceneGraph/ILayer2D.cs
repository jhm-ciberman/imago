using LifeSim.Imago.Rendering.Sprites;

namespace LifeSim.Imago.SceneGraph;

public interface ILayer2D
{
    void Draw(DrawingContext ctx);
    void Update(float deltaTime);
}
