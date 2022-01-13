using System;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class RenderTexture : IRenderTexture
{
    public event Action<IRenderTexture>? OnResized;

    public Framebuffer Framebuffer { get; private set; }

    public Veldrid.Texture DeviceTexture { get; private set; }

    public Veldrid.Texture DepthTexture { get; private set; }

    public Veldrid.Texture PickingTexture { get; private set; }

    public OutputDescription OutputDescription => this.Framebuffer.OutputDescription;

    public uint Width => this.Framebuffer.Width;
    public uint Height => this.Framebuffer.Height;

    public Sampler Sampler { get; private set; }

    private readonly GraphicsDevice _gd;

    public RenderTexture(GraphicsDevice gd, uint width, uint height)
    {
        this._gd = gd;
        this.DepthTexture = this.CreateDepthTexture(width, height);
        this.DeviceTexture = this.CreateColorTexture(width, height);
        this.PickingTexture = this.CreatePickingIDTexture(width, height);
        this.Framebuffer = this.CreateFramebuffer();
        this.Sampler = gd.LinearSampler;
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
            this.DepthTexture, this.DeviceTexture, this.PickingTexture
        ));
    }

    public void Resize(uint width, uint height)
    {
        this._gd.DisposeWhenIdle(this.DepthTexture);
        this._gd.DisposeWhenIdle(this.DeviceTexture);
        this._gd.DisposeWhenIdle(this.PickingTexture);
        this._gd.DisposeWhenIdle(this.Framebuffer);
        this.DepthTexture = this.CreateDepthTexture(width, height);
        this.DeviceTexture = this.CreateColorTexture(width, height);
        this.PickingTexture = this.CreatePickingIDTexture(width, height);
        this.Framebuffer = this.CreateFramebuffer();
        this.OnResized?.Invoke(this);
    }

    public void Dispose()
    {
        this.DepthTexture?.Dispose();
        this.DeviceTexture?.Dispose();
        this.PickingTexture?.Dispose();
        this.Framebuffer?.Dispose();
    }
}