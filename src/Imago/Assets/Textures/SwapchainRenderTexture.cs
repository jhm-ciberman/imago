using System;
using NeoVeldrid;

namespace Imago.Assets.Textures;

internal class SwapchainRenderTexture : IRenderTexture, ITexture
{
    /// <summary>
    /// Occurs when the render texture is resized.
    /// </summary>
    public event EventHandler? Resized;

    public Framebuffer Framebuffer => this._swapchain.Framebuffer;

    public NeoVeldrid.Texture NativeTexture => this.Framebuffer.ColorTargets[0].Target;

    public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

    public uint Width => this.Framebuffer.Width;

    public uint Height => this.Framebuffer.Height;

    public Sampler NativeSampler { get; }

    private readonly Swapchain _swapchain;

    internal SwapchainRenderTexture(GraphicsDevice gd, Swapchain swapchain)
    {
        this.NativeSampler = gd.LinearSampler;
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
