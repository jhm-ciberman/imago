using System.Numerics;
using LifeSim.Imago.Materials;
using LifeSim.Imago.Meshes;

namespace LifeSim.Imago.SceneGraph.Nodes;

public class MeshRenderInfo
{
    /// <summary>
    /// Gets the material to use for rendering.
    /// </summary>
    public Material Material { get; }

    /// <summary>
    /// Gets the mesh to render.
    /// </summary>
    public Mesh Mesh { get; }

    /// <summary>
    /// Gets the texture scale and translation to use for rendering.
    /// </summary>
    public Vector4 TextureST { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="MeshRenderInfo"/> class.
    /// </summary>
    /// <param name="material">The material to use for rendering.</param>
    /// <param name="mesh">The mesh to render.</param>
    public MeshRenderInfo(Material material, Mesh mesh)
    {
        this.Material = material;
        this.Mesh = mesh;
        this.TextureST = new Vector4(1f, 1f, 0f, 0f);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MeshRenderInfo"/> class.
    /// </summary>
    /// <param name="material">The material to use for rendering.</param>
    /// <param name="mesh">The mesh to render.</param>
    /// <param name="textureST">The texture scale and translation to use for rendering.</param>
    public MeshRenderInfo(Material material, Mesh mesh, Vector4 textureST)
    {
        this.Material = material;
        this.Mesh = mesh;
        this.TextureST = textureST;
    }
}
