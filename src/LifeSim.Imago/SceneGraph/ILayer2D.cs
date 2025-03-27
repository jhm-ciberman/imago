using LifeSim.Imago.Rendering.Sprites;

namespace LifeSim.Imago.SceneGraph;

public interface ILayer2D
{
    public void Draw(DrawingContext ctx);
    public void Update(float deltaTime);
}
