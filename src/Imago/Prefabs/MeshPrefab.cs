using Imago.Rendering.Meshes;
using Imago.SceneGraph;

namespace Imago.Prefabs;

/// <summary>
/// A <see cref="WrapingPrefab"> for a single mesh.
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

    public Node3D Instantiate()
    {
        return new RenderNode3D(this._mesh);
    }
}
