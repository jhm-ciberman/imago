using System.Numerics;

namespace LifeSim.Rendering
{
    public class Renderable3D : Node3D
    {
        private GPUMesh _mesh;
        public GPUMesh mesh => this._mesh;

        private Material _material;
        public Material material
        {
            get => this._material;
            set
            {
                if (this._material != material) {
                    this._material.MarkAsUnused();
                    this._material = material;
                    material.MarkAsUsed();
                }
            }
        }

        public Renderable3D(GPUMesh mesh, Material material)
        {
            this._mesh = mesh;
            this._material = material;
        }
    }
}