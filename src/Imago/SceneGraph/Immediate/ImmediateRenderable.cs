using Imago.SceneGraph.Nodes;

namespace Imago.SceneGraph.Immediate;

/// <summary>
/// An object that can draw in the immediate mode using the <see cref="IImmediateRenderer"/>.
/// </summary>
public class ImmediateRenderable3D : Node3D
{
    /// <inheritdoc/>
    public override void Mount(Scene3D scene)
    {
        scene.AddImmediateRenderable(this);
        base.Mount(scene);
    }

    /// <inheritdoc/>
    public override void Unmount()
    {
        this.Scene3D!.RemoveImmediateRenderable(this);
        base.Unmount();
    }

    /// <summary>
    /// Draws the object in the immediate mode.
    /// </summary>
    /// <param name="renderer">The <see cref="IImmediateRenderer"/> to use.</param>
    public virtual void Render(IImmediateRenderer renderer)
    {
        //
    }
}
