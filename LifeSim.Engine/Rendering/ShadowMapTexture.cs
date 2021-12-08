using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class ShadowMapTexture : IRenderTexture
    {
        public event Action<IRenderTexture>? OnResized;
        public uint Width { get; private set; }

        public uint Height { get; private set; }

        public Veldrid.Texture DeviceTexture { get; private set; }

        public Sampler Sampler { get; private set; }

        public Framebuffer Framebuffer { get; private set; }

        public OutputDescription OutputDescription { get; private set; }

        private readonly GraphicsDevice _gd;

        public ShadowMapTexture(GraphicsDevice gd, uint width, uint height)
        {
            this._gd = gd;
            this.Width = width;
            this.Height = height;
            this.DeviceTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
            this.Sampler = gd.LinearSampler;
            this.Framebuffer = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
                this.DeviceTexture, Array.Empty<Veldrid.Texture>()
            ));
            this.OutputDescription = this.Framebuffer.OutputDescription;
        }

        public void Resize(uint width, uint height)
        {
            this._gd.DisposeWhenIdle(this.DeviceTexture);
            this._gd.DisposeWhenIdle(this.Framebuffer);
            this.Width = width;
            this.Height = height;
            this.DeviceTexture = this._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(width, height, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
            this.Framebuffer = this._gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
                this.DeviceTexture, Array.Empty<Veldrid.Texture>()
            ));
            this.OutputDescription = this.Framebuffer.OutputDescription;
            this.OnResized?.Invoke(this);
        }

        public void Dispose()
        {
            this.DeviceTexture.Dispose();
            this.Framebuffer.Dispose();
        }
    }
}