using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public abstract class Material
    {
        public delegate void OnMaterialDirtyCallback(Material material, bool texturesDirty, bool uniformsDirty);

        public static event OnMaterialDirtyCallback? onMaterialDirty;

        public Shader shader { get; private set; }

        private ResourceSet? _resourceSet = null;

        private bool _dirty = true;

        protected BindableResource[] _resources;

        protected Material(Shader shader)
        {
            this.shader = shader;

            this._resources = new BindableResource[this.shader.resourceCount];
        }

        internal Veldrid.ResourceSet GetMaterialResourceSet()
        {
            if (this._dirty || this._resourceSet == null) {
                this._dirty = false;
                this._resourceSet?.Dispose();
                foreach (var res in this._resources) {
                    System.Console.WriteLine(res);
                }
                this._resourceSet = this.shader.CreateResourceSet(this._resources);
            }

            return this._resourceSet; 
        }

        protected void _SetDirty()
        {
            this._dirty = true;
            Material.onMaterialDirty?.Invoke(this, true, false);
        }

        public void Dispose()
        {
            this._resourceSet?.Dispose();
        }
    }
}