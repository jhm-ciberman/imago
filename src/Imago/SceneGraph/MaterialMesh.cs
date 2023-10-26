using System.Numerics;
using Imago.Rendering;
using Imago.Rendering.Materials;

namespace Imago.SceneGraph;

public class MaterialMesh
{
    public Material Material { get; }
    public Mesh Mesh { get; }
    public Vector4 TextureST { get; }

    public MaterialMesh(Material material, Mesh mesh)
    {
        this.Material = material;
        this.Mesh = mesh;
        this.TextureST = new Vector4(1f, 1f, 0f, 0f);
    }

    public MaterialMesh(Material material, Mesh mesh, Vector4 textureST)
    {
        this.Material = material;
        this.Mesh = mesh;
        this.TextureST = textureST;
    }

    public void SetupRenderNode(RenderNode3D renderNode)
    {
        renderNode.Mesh = this.Mesh;
        renderNode.Material = this.Material;
        renderNode.TextureST = this.TextureST;
    }
}
