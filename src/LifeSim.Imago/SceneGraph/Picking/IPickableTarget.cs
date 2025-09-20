namespace LifeSim.Imago.SceneGraph.Picking;

/// <summary>
/// Defines a contract for objects that can respond to mouse interaction events during picking operations.
/// </summary>
public interface IPickableTarget
{
    /// <summary>
    /// Called when the mouse cursor enters the pickable object.
    /// </summary>
    /// <param name="hitInfo">Information about the pick hit.</param>
    public void MouseEnter(HitInfo hitInfo);

    /// <summary>
    /// Called when the mouse cursor moves over the pickable object.
    /// </summary>
    /// <param name="hitInfo">Information about the pick hit.</param>
    public void MouseMove(HitInfo hitInfo);

    /// <summary>
    /// Called when the mouse cursor leaves the pickable object.
    /// </summary>
    public void MouseLeave();
}
