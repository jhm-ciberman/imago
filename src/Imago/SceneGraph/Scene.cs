using Imago.Controls;
using Support;

namespace Imago.SceneGraph;

public class Scene
{
    public Stage3D? Stage3D { get; set; } = null;
    public StageUI? StageUI { get; set; } = null;


    /// <summary>
    /// Gets or sets the clear color of the stage. If null, the stage will not be cleared
    /// and the previous frame will be visible.
    /// </summary>
    public Color? ClearColor { get; set; } = Color.Black;

    public Scene()
    {
        //
    }

    public virtual void OnBeforeRender()
    {
        this.Stage3D?.OnBeforeRender();
    }

    public virtual void RenderImGui()
    {
        // Virtual method
    }

    public virtual void Update(float deltaTime)
    {
        this.Stage3D?.Update(deltaTime);
        this.StageUI?.Update(deltaTime);
    }
}
