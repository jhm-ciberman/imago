using System;
using System.Numerics;
using Imago.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace Imago.Assets.Textures;

/// <summary>
/// Represents a texture in GPU memory that is not stored in RAM.
/// </summary>
public class DirectTexture : ITexture
{
    /// <summary>
    /// Gets the width of the texture in pixels.
    /// </summary>
    public uint Width { get; }

    /// <summary>
    /// Gets the height of the texture in pixels.
    /// </summary>
    public uint Height { get; }

    /// <summary>
    /// Gets the underlying Veldrid texture resource.
    /// </summary>
    public Veldrid.Texture VeldridTexture { get; private set; }

    /// <summary>
    /// Gets the Veldrid sampler used for texture sampling.
    /// </summary>
    public Sampler VeldridSampler { get; }

    /// <summary>
    /// Gets the number of mip levels in the texture.
    /// </summary>
    public uint MipLevels { get; }

    private readonly GraphicsDevice _gd;

    /// <summary>
    /// Occurs when the texture is resized.
    /// </summary>
    public event EventHandler? Resized;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectTexture"/> class.
    /// </summary>
    /// <param name="gd">The graphics device to use for creating the texture.</param>
    /// <param name="width">The width of the texture in pixels.</param>
    /// <param name="height">The height of the texture in pixels.</param>
    /// <param name="mipLevels">The number of mip levels. If 0, mip levels are calculated automatically.</param>
    /// <param name="srgb">Whether to use sRGB color space.</param>
    public DirectTexture(GraphicsDevice gd, uint width, uint height, uint mipLevels = 0, bool srgb = true)
    {
        this._gd = gd;
        this.Width = width;
        this.Height = height;
        this.MipLevels = mipLevels == 0
            ? (uint)Math.Max(1, BitOperations.Log2(Math.Min(width, height)))
            : mipLevels;

        this.VeldridTexture = this._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            this.Width, this.Height, this.MipLevels, 1,
            srgb ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb : PixelFormat.R8_G8_B8_A8_UNorm,
            TextureUsage.Sampled | TextureUsage.GenerateMipmaps
        ));
        this.VeldridSampler = this._gd.PointSampler;
    }

    /// <summary>
    /// Updates the entire texture with data from the specified image.
    /// </summary>
    /// <param name="image">The image to update the texture with.</param>
    public unsafe void Update(Image<Rgba32> image)
    {
        if (image.Width != this.Width || image.Height != this.Height)
            throw new ArgumentException("Image size does not match texture size.");

        byte[] buffer = new byte[image.Width * image.Height * sizeof(Rgba32)];
        image.CopyPixelDataTo(buffer);

        fixed (void* ptr = buffer)
        {
            this._gd.UpdateTexture(
                this.VeldridTexture, (nint)ptr, (uint)buffer.Length,
                x: 0, y: 0, z: 0,
                width: this.Width, height: this.Height, depth: 1,
                mipLevel: 0, arrayLayer: 0
            );
        }

        this.RegenerateMipMaps();
    }

    /// <summary>
    /// Updates a rectangular area of the texture with data from the specified image.
    /// </summary>
    /// <param name="image">The image to update the texture area with.</param>
    /// <param name="x">The X coordinate of the area to update.</param>
    /// <param name="y">The Y coordinate of the area to update.</param>
    public unsafe void UpdateArea(Image<Rgba32> image, uint x, uint y)
    {
        if (x + image.Width > this.Width || y + image.Height > this.Height)
            throw new ArgumentException($"The size of the rectangle to update is larger than the texture. The area to update is ({x}, {y}) to ({x + image.Width}, {y + image.Height}) and the texture is {this.Width}x{this.Height}.");

        byte[] buffer = new byte[image.Width * image.Height * sizeof(Rgba32)];
        image.CopyPixelDataTo(buffer);

        fixed (void* ptr = buffer)
        {
            this._gd.UpdateTexture(
                this.VeldridTexture, (nint)ptr, (uint)buffer.Length,
                x: x, y: y, z: 0,
                width: (uint)image.Width, height: (uint)image.Height, depth: 1,
                mipLevel: 0, arrayLayer: 0
            );
        }

        this.RegenerateMipMaps();
    }

    /// <summary>
    /// Updates a rectangular area of the texture with raw byte data.
    /// </summary>
    /// <param name="bytes">The raw byte data to update the texture with.</param>
    /// <param name="x">The X coordinate of the area to update.</param>
    /// <param name="y">The Y coordinate of the area to update.</param>
    /// <param name="width">The width of the area to update.</param>
    /// <param name="height">The height of the area to update.</param>
    public unsafe void Update(byte[] bytes, int x, int y, int width, int height)
    {
        if (x + width > this.Width || y + height > this.Height)
            throw new ArgumentException($"The size of the rectangle to update is larger than the texture. The area to update is ({x}, {y}) to ({x + width}, {y + height}) and the texture is {this.Width}x{this.Height}.");

        fixed (void* ptr = &bytes[0])
        {
            this._gd.UpdateTexture(this.VeldridTexture, (nint)ptr, (uint)bytes.Length,
                x: (uint)x, y: (uint)y, z: 0, width: (uint)width, height: (uint)height, depth: 1, mipLevel: 0, arrayLayer: 0);
        }

        this.RegenerateMipMaps();
    }


    /// <summary>
    /// Regenerates the mip map levels for the texture.
    /// </summary>
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

    /// <summary>
    /// Resizes the texture to the specified dimensions.
    /// </summary>
    /// <param name="width">The new width of the texture.</param>
    /// <param name="height">The new height of the texture.</param>
    public void Resize(uint width, uint height)
    {
        Renderer.Instance.DisposeWhenIdle(this.VeldridTexture);
        this.VeldridTexture = this._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            width, height, this.MipLevels, 1,
            PixelFormat.R8_G8_B8_A8_UNorm_SRgb,
            TextureUsage.Sampled | TextureUsage.GenerateMipmaps
        ));
        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Disposes the texture and releases associated resources.
    /// </summary>
    public void Dispose()
    {
        this.VeldridTexture.Dispose();
    }
}
