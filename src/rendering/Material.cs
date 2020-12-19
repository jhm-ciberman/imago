using Veldrid;

namespace LifeSim.Rendering
{
    public class Material : System.IDisposable
    {
        private Shader _shader;
        private Texture _texture;
        private ResourceSet _textureSet;

        public Material(ResourceFactory factory, GraphicsDevice graphicsDevice, Shader shader, DeviceBuffer worldBuffer, Texture texture) 
        {
            this._shader = shader;
            
            this._textureSet = factory.CreateResourceSet(
                new ResourceSetDescription(shader.worldTextureLayout, worldBuffer, texture.textureView, graphicsDevice.PointSampler)
            );
        }

        public Pipeline pipeline => this._shader.pipeline;
        public ResourceSet textureSet => this._textureSet;

        public void SetTexture(Texture texture)
        {
            this._texture = texture;
        }

        public void Dispose()
        {
            this._shader.Dispose();
            this._textureSet.Dispose();
        }

        ~Material() {
            this.Dispose();
        }
    }
}