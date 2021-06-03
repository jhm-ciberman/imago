using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class SurfaceMaterial : Material
    {
        public Shader shadowmapShader { get; private set; }

        public SurfaceMaterial(Shader shader, Shader shadowmapShader, Texture texture) : base(shader)
        {
            this._texture = texture;
            this._resources[0] = texture.deviceTexture;
            this._resources[1] = texture.sampler;
            this.shadowmapShader = shadowmapShader;
        }

        private Texture _texture;
        public Texture texture 
        { 
            get => this._texture; 
            set 
            {
                if (this._texture == value) return;
                
                this._texture = value; 
                this._resources[0] = value.deviceTexture;
                this._resources[1] = value.sampler;
                this._SetDirty();
            } 
        }
    }
}