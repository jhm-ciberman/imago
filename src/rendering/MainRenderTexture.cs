using Veldrid;

namespace LifeSim.Rendering
{
    public class MainRenderTexture : System.IDisposable
    {
        private Framebuffer _framebuffer;
        private Texture _depthTexture;
        private Texture _colorTexture;

        public MainRenderTexture(ResourceFactory factory, uint width, uint height)
        {
            this._depthTexture = factory.CreateTexture(new TextureDescription(
                width, height, depth: 1, mipLevels: 1, arrayLayers: 1, 
                PixelFormat.R16_UNorm, 
                TextureUsage.DepthStencil | TextureUsage.RenderTarget, 
                TextureType.Texture2D
            ));

            this._colorTexture = factory.CreateTexture(new TextureDescription(
                width, height, depth: 1, mipLevels: 1, arrayLayers: 1, 
                PixelFormat.R8_G8_B8_A8_UNorm, 
                TextureUsage.RenderTarget | TextureUsage.Sampled, 
                TextureType.Texture2D
            ));

            this._framebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                this._depthTexture, this._colorTexture
            ));
        }

        public Framebuffer framebuffer => this._framebuffer;

        public Texture colorTexture => this._colorTexture;
        
        public Texture depthTexture => this._depthTexture;
        
        public OutputDescription outputDescription => this._framebuffer.OutputDescription;

        public void Dispose()
        {
            this._depthTexture?.Dispose();
            this._colorTexture?.Dispose();
            this._framebuffer?.Dispose();
        }
    }
}