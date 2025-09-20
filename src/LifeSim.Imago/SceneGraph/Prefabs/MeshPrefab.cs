using LifeSim.Imago.Meshes;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.SceneGraph.Prefabs;

/// <summary>
/// A <see cref="WrapingPrefab" /> for a single mesh.
/// </summary>
public class MeshPrefab : IInstantiable
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

    /// <inheritdoc/>
    public Node3D Instantiate()
    {
        return new RenderNode3D(this._mesh);
    }
}
