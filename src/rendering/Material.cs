using Veldrid;

namespace LifeSim.Rendering
{
    public class Material
    {
        internal static System.Action<Material>? onRefCountZero;

        private MaterialPass _pass;
        public MaterialPass pass => this._pass;
        
        private GPUTexture _texture;
        public GPUTexture texture => this._texture;

        private uint _refCount = 0;

        public ResourceSet? resourceSet;
        public bool resourceSetIsDirty = true;

        public Material(Shader shader, GPUTexture texture) 
        {
            this._pass = new MaterialPass(shader);
            this._texture = texture;
        }

        public void SetTexture(GPUTexture texture)
        {
            this._texture = texture;
        }

        internal void MarkAsUsed()
        {
            this._refCount++;
        }

        internal void MarkAsUnused()
        {
            this._refCount--;
            if (this._refCount == 0) {
                Material.onRefCountZero?.Invoke(this);
            }
        }

    }
}