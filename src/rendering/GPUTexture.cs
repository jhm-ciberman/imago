namespace LifeSim.Rendering
{
    public class GPUTexture : System.IDisposable
    {
        protected Veldrid.Texture _deviceTexture;
        protected Veldrid.TextureView _textureView;
        protected Veldrid.Sampler _sampler;

        public GPUTexture(Veldrid.Texture texture, Veldrid.TextureView textureView, Veldrid.Sampler sampler)
        {
            this._deviceTexture = texture;
            this._textureView = textureView;
            this._sampler = sampler;
        }

        public uint width => this._deviceTexture.Width;
        public uint height => this._deviceTexture.Height;

        public Veldrid.TextureView textureView => this._textureView;
        public Veldrid.Texture deviceTexture => this._deviceTexture;
        public Veldrid.Sampler sampler => this._sampler;

        public void Dispose()
        {
            this._deviceTexture.Dispose();
            this._textureView.Dispose();
        }

    }
}