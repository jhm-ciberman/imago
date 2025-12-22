using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.SceneGraph.Immediate;

/// <summary>
/// An object that can draw in the immediate mode using the <see cref="IImmediateRenderer"/>.
/// </summary>
public class ImmediateRenderable3D : Node3D
{
    /// <inheritdoc/>
    public override void AttachToLayer(Layer3D layer)
    {
        layer.AddImmediateRenderable(this);
        base.AttachToLayer(layer);
    }

    /// <inheritdoc/>
    public override void DetachFromLayer()
    {
        this.Layer3D!.RemoveImmediateRenderable(this);
        base.DetachFromLayer();
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
