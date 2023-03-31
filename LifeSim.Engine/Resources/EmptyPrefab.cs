using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Resources;

/// <summary>
/// A <see cref="WrapingPrefab"> for an empty scene. This is used to create a scene
/// that does not contain any models.
/// </summary>
public class EmptyPrefab : IInstantiable
{
    public Node3D Instantiate()
    {
        return new Node3D();
    }
}
