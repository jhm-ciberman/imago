using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace LifeSim.Engine.Rendering;

/// <summary>
/// Represents a texture in GPU memory that is not stored in RAM.
/// </summary>
public class DirectTexture : ITexture
{
    public uint Width { get; }

    public uint Height { get; }

    public Veldrid.Texture VeldridTexture { get; private set; }

    public Sampler VeldridSampler { get; }

    public uint MipLevels { get; }

    private readonly GraphicsDevice _gd;

    public event EventHandler? Resized;

    public DirectTexture(uint width, uint height, uint mipLevels = 0, bool srgb = true)
    {
        this._gd = Renderer.Instance.GraphicsDevice;
        this.Width = width;
        this.Height = height;
        this.MipLevels = (mipLevels == 0)
            ? (uint)BitOperations.Log2(Math.Min(width, height))
            : mipLevels;

        this.VeldridTexture = this._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            this.Width, this.Height, this.MipLevels, 1,
            srgb ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb : PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled | TextureUsage.GenerateMipmaps
        ));
        this.VeldridSampler = this._gd.PointSampler;
    }

    public unsafe void Update(Image<Rgba32> image)
    {
        if (image.Width != this.Width || image.Height != this.Height)
        {
            throw new ArgumentException("Image size does not match texture size.");
        }

        if (!image.TryGetSinglePixelSpan(out Span<Rgba32> span))
        {
            throw new InvalidOperationException("Image is not in RGBA32 format.");
        }

        fixed (void* ptr = &span.GetPinnableReference())
        {
            this._gd.UpdateTexture(this.VeldridTexture, (IntPtr)ptr, (uint)(span.Length * sizeof(Rgba32)),
                x: 0, y: 0, z: 0, width: this.Width, height: this.Height, depth: 1, mipLevel: 0, arrayLayer: 0);
        }

        this.RegenerateMipMaps();
    }

    public unsafe void UpdateArea(Image<Rgba32> image, uint x, uint y)
    {
        if (x + image.Width > this.Width || y + image.Height > this.Height)
        {
            throw new ArgumentException($"The size of the rectangle to update is larger than the texture. The area to update is ({x}, {y}) to ({x + image.Width}, {y + image.Height}) and the texture is {this.Width}x{this.Height}.");
        }

        if (!image.TryGetSinglePixelSpan(out Span<Rgba32> span))
        {
            throw new InvalidOperationException("Image is not in RGBA32 format.");
        }

        fixed (void* ptr = &span.GetPinnableReference())
        {
            this._gd.UpdateTexture(this.VeldridTexture, (IntPtr)ptr, (uint)(span.Length * sizeof(Rgba32)),
                x: x, y: y, z: 0, width: (uint)image.Width, height: (uint)image.Height, depth: 1, mipLevel: 0, arrayLayer: 0);
        }

        this.RegenerateMipMaps();
    }

    public unsafe void Update(byte[] bytes, int x, int y, int width, int height)
    {
        if (x + width > this.Width || y + height > this.Height)
        {
            throw new ArgumentException($"The size of the rectangle to update is larger than the texture. The area to update is ({x}, {y}) to ({x + width}, {y + height}) and the texture is {this.Width}x{this.Height}.");
        }

        fixed (void* ptr = &bytes[0])
        {
            this._gd.UpdateTexture(this.VeldridTexture, (IntPtr)ptr, (uint)bytes.Length,
                x: (uint)x, y: (uint)y, z: 0, width: (uint)width, height: (uint)height, depth: 1, mipLevel: 0, arrayLayer: 0);
        }

        this.RegenerateMipMaps();
    }


    public void RegenerateMipMaps()
    {
        if (this.MipLevels > 1)
        {
            var cl = this._gd.ResourceFactory.CreateCommandList();
            cl.Begin();
            cl.GenerateMipmaps(this.VeldridTexture);
            cl.End();
            this._gd.SubmitCommands(cl);
            cl.Dispose();
        }
    }

    public void Resize(uint width, uint height)
    {
        this._gd.DisposeWhenIdle(this.VeldridTexture);
        this.VeldridTexture = this._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            width, height, this.MipLevels, 1,
            PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
            TextureUsage.Sampled | TextureUsage.GenerateMipmaps
        ));
        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        this.VeldridTexture.Dispose();
    }
}
