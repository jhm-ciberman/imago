using System.Numerics;
using LifeSim.Rendering;
using Veldrid;

namespace LifeSim.SceneGraph
{
    public class Renderable3D : Node3D, IRenderable
    {
        public System.UInt32 pickingID = 0;
        
        public GPUMesh mesh;
        
        public SurfaceMaterial material;
        
        public Vector4 albedoColor = new Vector4(1f, 1f, 1f, 1f);
        
        public ResourceLayout resourceLayout { get; private set; }

        public VertexLayoutKind vertexLayoutKind => this.mesh.vertexLayoutKind;

        public Renderable3D(GPUMesh mesh, SurfaceMaterial material)
        {
            this.mesh = mesh;
            this.material = material;
            this.resourceLayout = material.GetObjectResourceLayout(this);
        }

        public virtual string[] GetShaderKeywords()
        {
            return System.Array.Empty<string>();
        }
    }
}