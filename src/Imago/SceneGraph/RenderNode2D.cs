using Imago.Rendering;

namespace Imago.SceneGraph;

public abstract class RenderNode2D : Node2D, ICanvasItem
{
    public abstract void Render(SpriteBatcher spriteBatcher);
}
