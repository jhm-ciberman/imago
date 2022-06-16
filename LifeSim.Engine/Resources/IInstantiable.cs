using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Resources;

/// <summary>
/// Interface for resources that can be instantiated.
/// </summary>
public interface IInstantiable
{
    /// <summary>
    /// Instantiates the resource.
    /// </summary>
    /// <returns>The root node of the scene.</returns>
    Node3D Instantiate();
}