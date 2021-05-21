using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class FullscreenMaterial : Material
    {
        public FullscreenMaterial(Shader shader, IRenderTexture texture) : base(shader)
        {
            this._texture = texture;
            this._texture.onResized += this._OnSourceTextureResized;
            this._resources[0] = texture.colorTexture;
            this._resources[1] = this.shader._gd.LinearSampler;
        }

        private IRenderTexture _texture;
        public IRenderTexture texture 
        { 
            get => this._texture; 
            set 
            { 
                if (this._texture == value) return;
                
                this._texture.onResized -= this._OnSourceTextureResized;
                this._texture = value;
                this._texture.onResized += this._OnSourceTextureResized;

                this._resources[0] = value.colorTexture;
                this._resources[1] = this.shader._gd.LinearSampler;
                this._SetDirty();
            } 
        }

        private void _OnSourceTextureResized(IRenderTexture renderTexture)
        {
            this._resources[0] = renderTexture.colorTexture;
            this._resources[1] = this.shader._gd.LinearSampler;
            this._SetDirty();
        }

    }
}