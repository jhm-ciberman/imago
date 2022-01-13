using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph;

public abstract class RenderNode2D : Node2D, ICanvasItem
{
    public abstract void Render(SpriteBatcher spriteBatcher);
}