using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

internal class SwapchainRenderTexture : IRenderTexture, ITexture
{
    /// <inheritdoc />
    public event EventHandler? Resized;

    public Framebuffer Framebuffer => this._swapchain.Framebuffer;

    public Veldrid.Texture DeviceTexture => this.Framebuffer.ColorTargets[0].Target;

    public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

    public uint Width => this.Framebuffer.Width;

    public uint Height => this.Framebuffer.Height;

    public Sampler Sampler => this._gd.LinearSampler;

    private readonly Swapchain _swapchain;

    private readonly GraphicsDevice _gd;

    internal SwapchainRenderTexture()
    {
        this._gd = Renderer.Instance.GraphicsDevice;
        this._swapchain = this._gd.MainSwapchain;
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