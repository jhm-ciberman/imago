namespace LifeSim.Rendering
{
    public class Renderable
    {
        public Transform transform = new Transform();
        private Mesh _mesh;
        public Mesh mesh => this._mesh;

        private Material _material;
        public Material material => this._material;

        public Renderable(Mesh mesh, Material material)
        {
            this._mesh = mesh;
            this._material = material;
        }
    }
}