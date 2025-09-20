using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// An object that can draw in the immediate mode using the <see cref="IImediateRenderer"/>.
/// </summary>
public class ImmediateRenderable3D : Node3D
{
    /// <inheritdoc/>
    public override void AttachToStage(Stage stage)
    {
        stage.AddImmediateRenderable(this);
        base.AttachToStage(stage);
    }

    /// <inheritdoc/>
    public override void DetachFromStage()
    {
        this.Stage!.RemoveImmediateRenderable(this);
        base.DetachFromStage();
    }

    /// <summary>
    /// Draws the object in the immediate mode.
    /// </summary>
    /// <param name="renderer">The <see cref="IImediateRenderer"/> to use.</param>
    public virtual void Render(IImediateRenderer renderer)
    {
        //
    }
}
