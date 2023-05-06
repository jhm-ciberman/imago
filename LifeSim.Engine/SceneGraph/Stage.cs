using LifeSim.Engine.Controls;
using LifeSim.Support;

namespace LifeSim.Engine.SceneGraph;

public class Scene
{
    public Stage3D? Stage3D { get; set; } = null;

    public Stage2D? Stage2D { get; set; } = null;

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
        //this.Stage2D?.OnBeforeRender(); // Not implemented
    }

    public virtual void RenderImGui()
    {
        // Virtual method
    }

    public void UpdateTransforms()
    {
        this.Stage3D?.UpdateTransforms();
        this.Stage2D?.UpdateTransforms();
    }

    public virtual void Update(float deltaTime)
    {
        this.Stage3D?.Update(deltaTime);
        //this.Stage2D?.Update(deltaTime); // Not implemented
        this.StageUI?.Update(deltaTime);
    }
}
