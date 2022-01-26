using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

internal class SwapchainRenderTexture : IRenderTexture, ITexture
{
    public event Action<IRenderTexture>? OnResized;

    public Framebuffer Framebuffer => this._swapchain.Framebuffer;

    public Veldrid.Texture DeviceTexture => this.Framebuffer.ColorTargets[0].Target;

    public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

    public uint Width => this.Framebuffer.Width;

    public uint Height => this.Framebuffer.Height;

    public Sampler Sampler => this._gd.LinearSampler;

    private readonly Swapchain _swapchain;

    private readonly GraphicsDevice _gd;

    internal SwapchainRenderTexture(Renderer renderer, Swapchain swapchain)
    {
        this._gd = renderer.GraphicsDevice;
        this._swapchain = swapchain;
    }

    public void Resize(uint width, uint height)
    {
        this._swapchain.Resize(width, height);
        this.OnResized?.Invoke(this);
    }

    public void Dispose()
    {
        // Nothing because the swapchain is disposed when the window is closed.
    }
}