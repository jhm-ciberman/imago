using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class RenderTexture : IRenderTexture
    {
        private readonly Veldrid.ResourceFactory _factory;
        private Framebuffer _framebuffer;
        private Veldrid.Texture _colorTexture;

        public event Action<IRenderTexture>? OnResized;

        public RenderTexture(GraphicsDevice graphicsDevice, uint width, uint height)
        {
            this._factory = graphicsDevice.ResourceFactory;
            this.DepthTexture = this._CreateDepthTexture(width, height);
            this._colorTexture = this._CreateColorTexture(width, height);
            this.PickingTexture = this._CreatePickingIDTexture(width, height);
            this._framebuffer = this._CreateFramebuffer();
            this.Sampler = graphicsDevice.LinearSampler;
        }

        private Veldrid.Texture _CreateDepthTexture(uint width, uint height)
        {
            return this._factory.CreateTexture(new TextureDescription(
                width, height, depth: 1, mipLevels: 1, arrayLayers: 1,
                PixelFormat.D32_Float_S8_UInt,
                TextureUsage.DepthStencil | TextureUsage.Sampled,
                TextureType.Texture2D
            ));
        }

        private Veldrid.Texture _CreateColorTexture(uint width, uint height)
        {
            return this._factory.CreateTexture(new TextureDescription(
                width, height, depth: 1, mipLevels: 1, arrayLayers: 1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.RenderTarget | TextureUsage.Sampled,
                TextureType.Texture2D
            ));
        }

        private Veldrid.Texture _CreatePickingIDTexture(uint width, uint height)
        {
            return this._factory.CreateTexture(new TextureDescription(
                width, height, depth: 1, mipLevels: 1, arrayLayers: 1,
                PixelFormat.R32_UInt,
                TextureUsage.RenderTarget | TextureUsage.Sampled,
                TextureType.Texture2D
            ));
        }

        private Framebuffer _CreateFramebuffer()
        {
            return this._factory.CreateFramebuffer(new FramebufferDescription(
                this.DepthTexture, this._colorTexture, this.PickingTexture
            ));
        }

        public Framebuffer Framebuffer => this._framebuffer;

        public Veldrid.Texture DeviceTexture => this._colorTexture;

        public Veldrid.Texture DepthTexture { get; private set; }

        public Veldrid.Texture PickingTexture { get; private set; }

        public OutputDescription OutputDescription => this._framebuffer.OutputDescription;

        public int Width => (int)this.Framebuffer.Width;
        public int Height => (int)this.Framebuffer.Height;

        public Sampler Sampler { get; private set; }

        public void Dispose()
        {
            this.DepthTexture?.Dispose();
            this._colorTexture?.Dispose();
            this.PickingTexture?.Dispose();
            this._framebuffer?.Dispose();
        }

        public void Resize(uint width, uint height)
        {
            this.Dispose();
            this.DepthTexture = this._CreateDepthTexture(width, height);
            this._colorTexture = this._CreateColorTexture(width, height);
            this.PickingTexture = this._CreatePickingIDTexture(width, height);
            this._framebuffer = this._CreateFramebuffer();
            this.OnResized?.Invoke(this);
        }
    }
}