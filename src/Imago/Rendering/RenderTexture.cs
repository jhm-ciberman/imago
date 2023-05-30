using System;
using Veldrid;

namespace Imago.Rendering;

public class RenderTexture : IRenderTexture
{
    /// <summary>
    /// Raised when the render texture is resized.
    /// </summary>
    public event EventHandler? Resized;

    public Framebuffer Framebuffer { get; private set; }

    public Framebuffer ColorOnlyFramebuffer { get; private set; }

    public Veldrid.Texture VeldridTexture { get; private set; }

    public Veldrid.Texture DepthTexture { get; private set; }

    public Veldrid.Texture PickingTexture { get; private set; }

    public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

    public uint Width => this.Framebuffer.Width;
    public uint Height => this.Framebuffer.Height;

    public Sampler VeldridSampler { get; private set; }

    private readonly GraphicsDevice _gd;

    private readonly Renderer _renderer;

    internal RenderTexture(Renderer renderer, uint width, uint height)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        this.DepthTexture = this.CreateDepthTexture(width, height);
        this.VeldridTexture = this.CreateColorTexture(width, height);
        this.PickingTexture = this.CreatePickingIDTexture(width, height);
        this.Framebuffer = this.CreateFramebuffer();
        this.ColorOnlyFramebuffer = this.CreateColorOnlyFramebuffer();
        this.VeldridSampler = this._gd.LinearSampler;
    }

    private Veldrid.Texture CreateDepthTexture(uint width, uint height)
    {
        return this._gd.ResourceFactory.CreateTexture(new TextureDescription(
            width, height, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.D32_Float_S8_UInt,
            TextureUsage.DepthStencil | TextureUsage.Sampled,
            TextureType.Texture2D
        ));
    }

    private Veldrid.Texture CreateColorTexture(uint width, uint height)
    {
        return this._gd.ResourceFactory.CreateTexture(new TextureDescription(
            width, height, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.RenderTarget | TextureUsage.Sampled,
            TextureType.Texture2D
        ));
    }

    private Veldrid.Texture CreatePickingIDTexture(uint width, uint height)
    {
        return this._gd.ResourceFactory.CreateTexture(new TextureDescription(
            width, height, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R32_UInt,
            TextureUsage.RenderTarget | TextureUsage.Sampled,
            TextureType.Texture2D
        ));
    }

    private Framebuffer CreateFramebuffer()
    {
        return this._gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
            this.DepthTexture, this.VeldridTexture, this.PickingTexture
        ));
    }

    private Framebuffer CreateColorOnlyFramebuffer()
    {
        return this._gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
            this.DepthTexture, this.VeldridTexture
        ));
    }

    public void Resize(uint width, uint height)
    {
        this._renderer.DisposeWhenIdle(this.DepthTexture);
        this._renderer.DisposeWhenIdle(this.VeldridTexture);
        this._renderer.DisposeWhenIdle(this.PickingTexture);
        this._renderer.DisposeWhenIdle(this.Framebuffer);
        this._renderer.DisposeWhenIdle(this.ColorOnlyFramebuffer);
        this.DepthTexture = this.CreateDepthTexture(width, height);
        this.VeldridTexture = this.CreateColorTexture(width, height);
        this.PickingTexture = this.CreatePickingIDTexture(width, height);
        this.Framebuffer = this.CreateFramebuffer();
        this.ColorOnlyFramebuffer = this.CreateColorOnlyFramebuffer();
        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        this.DepthTexture?.Dispose();
        this.VeldridTexture?.Dispose();
        this.PickingTexture?.Dispose();
        this.Framebuffer?.Dispose();
        this.ColorOnlyFramebuffer?.Dispose();
    }
}
