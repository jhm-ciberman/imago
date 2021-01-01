using LifeSim.Rendering;

namespace LifeSim.SceneGraph
{
    public class Renderable3D : Node3D
    {
        public GPUMesh mesh;
        public Material material;

        public Renderable3D(GPUMesh mesh, Material material)
        {
            this.mesh = mesh;
            this.material = material;
        }
    }
}