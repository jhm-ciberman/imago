using System;
using LifeSim.Imago.Graphics.Textures;
using Veldrid;

namespace LifeSim.Imago.Graphics.Rendering.Shadows;

internal class ShadowMapTexture : ITexture, IDisposable
{
    /// <summary>
    /// Occurs when the texture is resized.
    /// </summary>
    public event EventHandler? Resized;

    uint ITexture.Width => this.Size;

    uint ITexture.Height => this.Size;

    public uint Size { get; private set; }

    public uint CascadesCount { get; private set; }

    public Veldrid.Texture VeldridTexture { get; private set; }

    public Sampler VeldridSampler { get; private set; }

    public Sampler ShadowSampler { get; private set; }

    public Framebuffer[] Framebuffers { get; private set; }

    private readonly Renderer _renderer;

    internal ShadowMapTexture(Renderer renderer, uint size, uint cascadesCount)
    {
        var gd = renderer.GraphicsDevice;
        this._renderer = renderer;
        this.Size = size;
        this.CascadesCount = cascadesCount;
        this.VeldridTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(this.Size, this.Size, 1, this.CascadesCount, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));

        this.VeldridSampler = gd.LinearSampler;

        this.ShadowSampler = gd.ResourceFactory.CreateSampler(new SamplerDescription(
            SamplerAddressMode.Border, SamplerAddressMode.Border, SamplerAddressMode.Border,
            SamplerFilter.MinLinear_MagLinear_MipPoint,
            ComparisonKind.Greater,
            0, 0, 0, 0, SamplerBorderColor.OpaqueWhite
        ));

        this.Framebuffers = new Framebuffer[this.CascadesCount];

        for (uint i = 0; i < this.CascadesCount; i++)
        {
            this.Framebuffers[i] = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(this.VeldridTexture, i),
                Array.Empty<FramebufferAttachmentDescription>()
            ));
        }
    }

    public void Dispose()
    {
        this.VeldridTexture.Dispose();
        this.VeldridSampler.Dispose();

        foreach (Framebuffer fb in this.Framebuffers)
        {
            fb.Dispose();
        }
    }

    internal void Resize(uint size, uint cascadesCount)
    {
        this.CascadesCount = cascadesCount;
        ((ITexture)this).Resize(size, size);
    }

    void ITexture.Resize(uint width, uint height)
    {
        if (width != height)
            throw new ArgumentException("Shadow map texture must be square.");

        this.Size = width;
        foreach (var framebuffer in this.Framebuffers)
        {
            this._renderer.DisposeWhenIdle(framebuffer);
        }
        this._renderer.DisposeWhenIdle(this.VeldridTexture);
        var factory = this._renderer.GraphicsDevice.ResourceFactory;
        this.VeldridTexture = factory.CreateTexture(TextureDescription.Texture2D(width, height, 1, this.CascadesCount, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));

        this.Framebuffers = new Framebuffer[this.CascadesCount];

        for (uint i = 0; i < this.CascadesCount; i++)
        {
            this.Framebuffers[i] = factory.CreateFramebuffer(new FramebufferDescription(
                new FramebufferAttachmentDescription(this.VeldridTexture, i),
                Array.Empty<FramebufferAttachmentDescription>()
            ));
        }

        this.Resized?.Invoke(this, EventArgs.Empty);
    }
}
