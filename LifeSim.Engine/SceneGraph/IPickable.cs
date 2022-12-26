namespace LifeSim.Engine.SceneGraph;

/// <summary>
/// An interface for objects that can be picked.
/// </summary>
public interface IPickable
{
    /// <summary>
    /// Gets the ID of the pickable object.
    /// </summary>
    uint PickId { get; set; }
}
