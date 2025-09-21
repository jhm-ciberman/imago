using System;
using LifeSim.Imago.Rendering;
using Veldrid;

namespace LifeSim.Imago.Textures;

/// <summary>
/// Defines the interpolation methods for texture sampling.
/// </summary>
public enum TextureInterpolation
{
    /// <summary>
    /// Nearest neighbor interpolation, providing sharp pixel boundaries.
    /// </summary>
    Nearest,

    /// <summary>
    /// Linear interpolation, providing smooth blending between pixels.
    /// </summary>
    Linear,
};

/// <summary>
/// Represents a texture that can be used as a rendering target.
/// </summary>
/// <remarks>
/// This class encapsulates the color and depth textures, as well as the framebuffers, required for off-screen rendering.
/// It also manages the resources for mouse picking.
/// </remarks>
public class RenderTexture : IRenderTexture
{
    /// <summary>
    /// Occurs when the render texture is resized.
    /// </summary>
    public event EventHandler? Resized;


    Veldrid.Texture ITexture.VeldridTexture => this.ForwardColorTexture;

    /// <summary>
    /// Gets the color texture for the main forward rendering pass.
    /// </summary>
    public Veldrid.Texture ForwardColorTexture { get; private set; } = null!;

    /// <summary>
    /// Gets the depth texture for the main forward rendering pass.
    /// </summary>
    public Veldrid.Texture ForwardDepthTexture { get; private set; } = null!;

    /// <summary>
    /// Gets the color texture used for the mouse picking pass.
    /// </summary>
    public Veldrid.Texture PickingColorTexture { get; private set; } = null!;

    /// <summary>
    /// Gets the depth texture used for the mouse picking pass.
    /// </summary>
    public Veldrid.Texture PickingDepthTexture { get; private set; } = null!;

    /// <summary>
    /// Gets the framebuffer for the main forward rendering pass.
    /// </summary>
    public Framebuffer Framebuffer { get; private set; } = null!;

    /// <summary>
    /// Gets the framebuffer for the mouse picking pass.
    /// </summary>
    public Framebuffer PickingFramebuffer { get; private set; } = null!;

    /// <summary>
    /// Gets the output description for the main framebuffer.
    /// </summary>
    public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

    /// <summary>
    /// Gets the output description for the picking framebuffer.
    /// </summary>
    public OutputDescription PickingOutputDescription => this.PickingFramebuffer.OutputDescription;

    /// <summary>
    /// Gets the width of the render texture in pixels.
    /// </summary>
    public uint Width { get; private set; }

    /// <summary>
    /// Gets the height of the render texture in pixels.
    /// </summary>
    public uint Height { get; private set; }

    /// <summary>
    /// Gets the sampler used for sampling the render texture.
    /// </summary>
    public Sampler VeldridSampler { get; private set; }

    /// <summary>
    /// Gets the multi-sample count for the render texture.
    /// </summary>
    public TextureSampleCount SampleCount { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this render texture has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; } = false;

    private readonly GraphicsDevice _gd;

    private readonly Renderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderTexture"/> class.
    /// </summary>
    /// <param name="width">The width of the render texture in pixels.</param>
    /// <param name="height">The height of the render texture in pixels.</param>
    /// <param name="sampleCount">The multi-sample count.</param>
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

    /// <summary>
    /// Resizes the render texture and recreates its underlying resources.
    /// </summary>
    /// <param name="width">The new width in pixels.</param>
    /// <param name="height">The new height in pixels.</param>
    public void Resize(uint width, uint height)
    {
        if (this.Width == width && this.Height == height) return;

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

    /// <summary>
    /// Disposes the render texture and releases its GPU resources.
    /// </summary>
    public void Dispose()
    {
        if (this.IsDisposed) return;
        this.IsDisposed = true;

        this.DisposeResources();
        this._renderer.UnregisterDisposable(this);
    }
}
