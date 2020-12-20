namespace LifeSim.Rendering
{
    public class GPUTexture : System.IDisposable
    {
        private Veldrid.Texture _deviceTexture;
        private Veldrid.TextureView _textureView;

        public GPUTexture(Veldrid.Texture texture, Veldrid.TextureView textureView)
        {
            this._deviceTexture = texture;
            this._textureView = textureView;
        }

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