using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

internal class SwapchainRenderTexture : IRenderTexture, ITexture
{
    /// <inheritdoc />
    public event EventHandler? Resized;

    public Framebuffer Framebuffer => this._swapchain.Framebuffer;

    public Veldrid.Texture VeldridTexture => this.Framebuffer.ColorTargets[0].Target;

    public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

    public uint Width => this.Framebuffer.Width;

    public uint Height => this.Framebuffer.Height;

    public Sampler VeldridSampler { get; }

    private readonly Swapchain _swapchain;

    internal SwapchainRenderTexture(GraphicsDevice gd, Swapchain swapchain)
    {
        this.VeldridSampler = gd.LinearSampler;
        this._swapchain = swapchain;
    }

    public void Resize(uint width, uint height)
    {
        this._swapchain.Resize(width, height);
        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        // Nothing because the swapchain is disposed when the window is closed.
    }
}
