namespace LifeSim.Rendering
{
    public class GPUTexture : System.IDisposable
    {
        protected Veldrid.Texture _deviceTexture;
        protected Veldrid.TextureView _textureView;

        public GPUTexture(Veldrid.Texture texture, Veldrid.TextureView textureView)
        {
            this._deviceTexture = texture;
            this._textureView = textureView;
        }

        public uint width => this._deviceTexture.Width;
        public uint height => this._deviceTexture.Height;

        public Veldrid.TextureView textureView => this._textureView;

        public void Dispose()
        {
            this._deviceTexture.Dispose();
            this._textureView.Dispose();
        }

        ~GPUTexture() {
            this.Dispose();
        }

    }
}