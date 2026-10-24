using Imago.Graphics3D.SceneGraph.Nodes;

namespace Imago.Graphics3D.SceneGraph.Prefabs;

/// <summary>
/// Interface for resources that can be instantiated.
/// </summary>
public interface IInstantiable
{
    /// <summary>
    /// Instantiates the resource.
    /// </summary>
    /// <returns>The root node of the scene.</returns>
    public Node3D Instantiate();
}
