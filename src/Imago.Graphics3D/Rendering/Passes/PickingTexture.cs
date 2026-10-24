using System;
using Imago.Rendering;
using Imago.Textures;
using NeoVeldrid;

namespace Imago.Graphics3D.Rendering.Passes;

/// <summary>
/// Holds the textures and the framebuffer that the mouse picking pass draws into.
/// It always has the same size as its source render texture.
/// </summary>
internal class PickingTexture : IDisposable
{
    /// <summary>
    /// Gets the texture that holds the pick id of each pixel.
    /// </summary>
    public NeoVeldrid.Texture ColorTexture { get; private set; } = null!;

    /// <summary>
    /// Gets the framebuffer to draw pick ids into.
    /// </summary>
    public Framebuffer Framebuffer { get; private set; } = null!;

    private readonly Renderer _renderer;

    private readonly RenderTexture _source;

    private NeoVeldrid.Texture _depthTexture = null!;

    private bool _isDepthShared;

    /// <summary>
    /// Initializes a new instance of the <see cref="PickingTexture"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="source">The render texture to follow in size.</param>
    public PickingTexture(Renderer renderer, RenderTexture source)
    {
        this._renderer = renderer;
        this._source = source;
        this._source.Resized += this.Source_Resized;
        this.CreateResources();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this._source.Resized -= this.Source_Resized;
        this.DisposeResources();
    }

    private void Source_Resized(object? sender, EventArgs e)
    {
        this.DisposeResources();
        this.CreateResources();
    }

    private void CreateResources()
    {
        var factory = this._renderer.GraphicsDevice.ResourceFactory;
        var source = this._source;

        this.ColorTexture = factory.CreateTexture(new TextureDescription(
            source.Width, source.Height, depth: 1, mipLevels: 1, arrayLayers: 1,
            PixelFormat.R32_UInt,
            TextureUsage.RenderTarget | TextureUsage.Sampled,
            TextureType.Texture2D,
            TextureSampleCount.Count1
        ));

        this._isDepthShared = source.SampleCount == TextureSampleCount.Count1;
        this._depthTexture = this._isDepthShared
            ? source.ForwardDepthTexture // We can share the depth texture if we don't need multisampling
            : factory.CreateTexture(new TextureDescription(
                source.Width, source.Height, depth: 1, mipLevels: 1, arrayLayers: 1,
                PixelFormat.D32_Float_S8_UInt,
                TextureUsage.DepthStencil | TextureUsage.Sampled,
                TextureType.Texture2D,
                TextureSampleCount.Count1
            ));

        this.Framebuffer = factory.CreateFramebuffer(new FramebufferDescription(this._depthTexture, this.ColorTexture));
    }

    private void DisposeResources()
    {
        this._renderer.DisposeWhenIdle(this.ColorTexture);
        this._renderer.DisposeWhenIdle(this.Framebuffer);

        if (!this._isDepthShared)
        {
            this._renderer.DisposeWhenIdle(this._depthTexture);
        }
    }
}
