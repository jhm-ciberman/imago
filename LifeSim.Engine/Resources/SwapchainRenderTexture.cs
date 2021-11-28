using System;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    internal class SwapchainRenderTexture : IRenderTexture, ITexture
    {
        private readonly Swapchain _swapchain;

        public SwapchainRenderTexture(Swapchain swapchain)
        {
            this._swapchain = swapchain;
        }

        public Framebuffer Framebuffer => this._swapchain.Framebuffer;

        public Veldrid.Texture DeviceTexture => this.Framebuffer.ColorTargets[0].Target;

        public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

        public int Width => (int)this.Framebuffer.Width;

        public int Height => (int)this.Framebuffer.Height;

        public Sampler Sampler => Renderer.Instance.GraphicsDevice.LinearSampler;

        public event Action<IRenderTexture>? OnResized;

        public void Dispose()
        {
            // 
        }

        public void Resize(uint width, uint height)
        {
            this._swapchain.Resize(width, height);
            this.OnResized?.Invoke(this);
        }
    }
}