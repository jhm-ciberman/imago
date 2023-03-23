using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Assets;

/// <summary>
/// A <see cref="Prefab"> for a single mesh.
/// </summary>
public class MeshPrefab : Prefab
{
    private readonly Mesh _mesh;

    /// <summary>
    /// Initializes a new instance of the <see cref="MeshPrefab"/> class.
    /// </summary>
    /// <param name="mesh">The mesh to use.</param>
    public MeshPrefab(Mesh mesh)
    {
        this._mesh = mesh;
    }

    protected override Node3D InstantiateCore()
    {
        return new RenderNode3D(this._mesh);
    }
}
