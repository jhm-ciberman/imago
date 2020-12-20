namespace LifeSim.Rendering
{
    public class Renderable
    {
        public Transform transform = new Transform();
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

        public Renderable(GPUMesh mesh, Material material)
        {
            this._mesh = mesh;
            this._material = material;
        }
    }
}