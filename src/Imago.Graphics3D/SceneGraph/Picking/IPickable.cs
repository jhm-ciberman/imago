namespace Imago.Graphics3D.SceneGraph.Picking;

/// <summary>
/// An interface for objects that can be picked.
/// </summary>
public interface IPickable
{
    /// <summary>
    /// Gets or sets the ID of the pickable object.
    /// </summary>
    public uint PickId { get; set; }
}
