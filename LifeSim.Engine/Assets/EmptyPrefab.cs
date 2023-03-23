using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Assets;

/// <summary>
/// A <see cref="Prefab"> for an empty scene. This is used to create a scene
/// that does not contain any models.
/// </summary>
public class EmptyPrefab : Prefab
{
    protected override Node3D InstantiateCore()
    {
        return new Node3D();
    }
}
