using System;
using LifeSim.Imago.Rendering;
using Veldrid;

namespace LifeSim.Imago.Textures;

public enum TextureInterpolation
{
    Nearest,
    Linear,
};

public class RenderTexture : IRenderTexture
{
    /// <summary>
    /// Occurs when the render texture is resized.
    /// </summary>
    public event EventHandler? Resized;


    Veldrid.Texture ITexture.VeldridTexture => this.ForwardColorTexture;

    public Veldrid.Texture ForwardColorTexture { get; private set; } = null!;
    public Veldrid.Texture ForwardDepthTexture { get; private set; } = null!;
    public Veldrid.Texture PickingColorTexture { get; private set; } = null!;
    public Veldrid.Texture PickingDepthTexture { get; private set; } = null!;
    public Framebuffer Framebuffer { get; private set; } = null!;
    public Framebuffer PickingFramebuffer { get; private set; } = null!;

    public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

    public OutputDescription PickingOutputDescription => this.PickingFramebuffer.OutputDescription;

    public uint Width { get; private set; }
    public uint Height { get; private set; }

    public Sampler VeldridSampler { get; private set; }
    public TextureSampleCount SampleCount { get; private set; }

    public bool IsDisposed { get; private set; } = false;

    private readonly GraphicsDevice _gd;

    private readonly Renderer _renderer;

    public RenderTexture(uint width, uint height, TextureSampleCount sampleCount = TextureSampleCount.Count1)
    {
        this._renderer = Renderer.Instance;
        this.SampleCount = sampleCount;
        this._gd = this._renderer.GraphicsDevice;
        this.Width = width;
        this.Height = height;

        this.RecreateResources();

        this.VeldridSampler = this._gd.LinearSampler;
        this._renderer.RegisterDisposable(this);
    }



    private void RecreateResources()
    {
        this.ForwardColorTexture = this._gd.ResourceFactory.CreateTexture(new TextureDescription(
            this.Width, this.Height, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.RenderTarget | TextureUsage.Sampled,
            TextureType.Texture2D,
            this.SampleCount
        ));

        this.ForwardDepthTexture = this._gd.ResourceFactory.CreateTexture(new TextureDescription(
            this.Width, this.Height, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.D32_Float_S8_UInt,
            TextureUsage.DepthStencil | TextureUsage.Sampled,
            TextureType.Texture2D,
            this.SampleCount
        ));

        this.PickingColorTexture = this._gd.ResourceFactory.CreateTexture(new TextureDescription(
            this.Width, this.Height, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R32_UInt,
            TextureUsage.RenderTarget | TextureUsage.Sampled,
            TextureType.Texture2D,
            TextureSampleCount.Count1
        ));

        this.PickingDepthTexture = (this.SampleCount == TextureSampleCount.Count1)
            ? this.ForwardDepthTexture // We can share the depth texture if we don't need multisampling
            : this._gd.ResourceFactory.CreateTexture(new TextureDescription(
                this.Width, this.Height, depth: 1, mipLevels: 1, arrayLayers: 1,
                PixelFormat.D32_Float_S8_UInt,
                TextureUsage.DepthStencil | TextureUsage.Sampled,
                TextureType.Texture2D,
                TextureSampleCount.Count1
            ));

        this.Framebuffer = this.CreateFramebuffer();
        this.PickingFramebuffer = this.CreatePickingFramebuffer();
    }

    private Framebuffer CreateFramebuffer()
    {
        return this._gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
            this.ForwardDepthTexture, this.ForwardColorTexture
        ));
    }

    private Framebuffer CreatePickingFramebuffer()
    {
        return this._gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
            this.PickingDepthTexture, this.PickingColorTexture
        ));
    }

    private void DisposeResources()
    {
        this._renderer.DisposeWhenIdle(this.ForwardColorTexture);
        this._renderer.DisposeWhenIdle(this.ForwardDepthTexture);
        this._renderer.DisposeWhenIdle(this.PickingColorTexture);
        this._renderer.DisposeWhenIdle(this.PickingDepthTexture);
        this._renderer.DisposeWhenIdle(this.Framebuffer);
        this._renderer.DisposeWhenIdle(this.PickingFramebuffer);
    }


    public void Resize(uint width, uint height)
    {
        this.Width = width;
        this.Height = height;

        this.DisposeResources();
        this.RecreateResources();

        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    private TextureInterpolation _interpolation = TextureInterpolation.Linear;

    /// <summary>
    /// Gets or sets the texture interpolation mode.
    /// </summary>
    public TextureInterpolation Interpolation
    {
        get => this._interpolation;
        set
        {
            if (this._interpolation == value) return;
            this._interpolation = value;
            this.VeldridSampler = this._interpolation switch
            {
                TextureInterpolation.Nearest => this._gd.PointSampler,
                TextureInterpolation.Linear => this._gd.LinearSampler,
                _ => throw new NotImplementedException(),
            };
        }
    }

    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;

        this.DisposeResources();
        this._renderer.UnregisterDisposable(this);
    }
}
