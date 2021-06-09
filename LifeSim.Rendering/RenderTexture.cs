using System;
using Veldrid;

namespace LifeSim.Rendering
{
    public interface IRenderTexture : System.IDisposable
    {
        event System.Action<IRenderTexture>? onResized;
        uint width {get;}
        uint height {get;}
        Framebuffer framebuffer {get;}
        Veldrid.Texture colorTexture {get;}
        OutputDescription outputDescription {get;}
        void Resize(uint width, uint height);
    }

    internal class SwapchainRenderTexture : IRenderTexture
    {
        private readonly Swapchain _swapchain; 
        
        public SwapchainRenderTexture(Swapchain swapchain)
        {
            this._swapchain = swapchain;
        }

        public Framebuffer framebuffer => this._swapchain.Framebuffer;

        public Veldrid.Texture colorTexture => this.framebuffer.ColorTargets[0].Target;

        public OutputDescription outputDescription => this.framebuffer.OutputDescription;

        public uint width => this.framebuffer.Width;

        public uint height => this.framebuffer.Height;

        public event Action<IRenderTexture>? onResized;

        public void Dispose()
        {
            // 
        }

        public void Resize(uint width, uint height)
        {
            this._swapchain.Resize(width, height);
            this.onResized?.Invoke(this);
        }
    }

    public class RenderTexture : IRenderTexture
    {
        private readonly Veldrid.ResourceFactory _factory;
        private Framebuffer _framebuffer;
        private Veldrid.Texture _depthTexture;
        private Veldrid.Texture _colorTexture;
        private Veldrid.Texture _pickingTexture;

        public event Action<IRenderTexture>? onResized;

        public RenderTexture(Veldrid.ResourceFactory factory, uint width, uint height)
        {
            this._factory = factory;
            this._depthTexture = this._CreateDepthTexture(width, height);
            this._colorTexture = this._CreateColorTexture(width, height);
            this._pickingTexture = this._CreatePickingIDTexture(width, height);
            this._framebuffer = this._CreateFramebuffer();
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
                this._depthTexture, this._colorTexture, this._pickingTexture
            ));
        }

        public Framebuffer framebuffer => this._framebuffer;

        public Veldrid.Texture colorTexture => this._colorTexture;
        
        public Veldrid.Texture depthTexture => this._depthTexture;

        public Veldrid.Texture pickingTexture => this._pickingTexture;
        
        public OutputDescription outputDescription => this._framebuffer.OutputDescription;

        public uint width => this.framebuffer.Width;
        public uint height => this.framebuffer.Height;

        public void Dispose()
        {
            this._depthTexture?.Dispose();
            this._colorTexture?.Dispose();
            this._pickingTexture?.Dispose();
            this._framebuffer?.Dispose();
        }

        public void Resize(uint width, uint height)
        {
            this.Dispose();
            this._depthTexture = this._CreateDepthTexture(width, height);
            this._colorTexture = this._CreateColorTexture(width, height);
            this._pickingTexture = this._CreatePickingIDTexture(width, height);
            this._framebuffer = this._CreateFramebuffer();
            this.onResized?.Invoke(this);
        }
    }
}