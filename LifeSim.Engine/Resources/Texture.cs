using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class Texture : ITexture
{
    public Veldrid.Texture DeviceTexture { get; protected set; }
    public Sampler Sampler { get; private set; }
    public uint Width { get; protected set; }
    public uint Height { get; protected set; }
    public uint MipLevels { get; protected set; }

    private readonly byte[] _data;

    private bool _isDirty = false;

    protected readonly Renderer _renderer;

    public Texture(Renderer renderer, uint width, uint height, uint mipLevels = 0, bool srgb = true)
    {
        this._renderer = renderer;
        this.Width = width;
        this.Height = height;

        this.MipLevels = (mipLevels == 0)
            ? (uint)BitOperations.Log2(Math.Min(width, height))
            : mipLevels;

        this._data = new byte[width * height * 4];
        PixelFormat pixelFormat = srgb ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb : PixelFormat.R8_G8_B8_A8_UNorm;

        var gd = this._renderer.GraphicsDevice;
        this.DeviceTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            this.Width, this.Height, this.MipLevels, 1,
            pixelFormat, TextureUsage.Sampled | TextureUsage.GenerateMipmaps
        ));
        this.Sampler = gd.PointSampler;
        renderer.OnTextureDirty(this);
    }

    protected void OnTextureDirty()
    {
        if (!this._isDirty)
        {
            this._isDirty = true;
            this._renderer.OnTextureDirty(this);
        }
    }

    public void Fill(Color fillColor)
    {
        for (int i = 0; i < this._data.Length; i += 4)
        {
            this._data[i] = fillColor.R;
            this._data[i + 1] = fillColor.G;
            this._data[i + 2] = fillColor.B;
            this._data[i + 3] = fillColor.A;
        }
        this.OnTextureDirty();
    }

    public unsafe void SetDataFromImage(Image<Rgba32> img)
    {
        if (img.Width != this.Width || img.Height != this.Height)
        {
            throw new ArgumentException("Image size does not match texture size.");
        }

        if (!img.TryGetSinglePixelSpan(out Span<Rgba32> pixels))
        {
            throw new ArgumentException("Image is not in RGBA32 format.");
        }

        var dest = new Span<byte>(this._data);
        MemoryMarshal.Cast<Rgba32, byte>(pixels).CopyTo(dest);

        this.OnTextureDirty();
    }

    public unsafe void SetDataFromBytes(int x, int y, int width, int height, byte[] data)
    {
        if (x + width > this.Width || y + height > this.Height)
        {
            throw new ArgumentException("Texture size does not match data size.");
        }

        if (x == 0 && y == 0 && width == this.Width && height == this.Height)
        {
            data.CopyTo(this._data, 0);
        }
        else
        {
            // Copy each row of data into the correct place in the texture
            for (int row = 0; row < height; row++)
            {
                var src = new Span<byte>(data, row * width * 4, width * 4);
                var dest = new Span<byte>(this._data, (int)((y + row) * this.Width * 4 + x * 4), width * 4);
                src.CopyTo(dest);
            }
        }

        this.OnTextureDirty();
    }

    public void Update(GraphicsDevice gd, CommandList cl)
    {
        gd.UpdateTexture(this.DeviceTexture, this._data,
            x: 0, y: 0, z: 0,
            width: (uint)this.Width, height: (uint)this.Height, depth: 1,
            arrayLayer: 0, mipLevel: 0);

        if (this.MipLevels > 1)
        {
            cl.GenerateMipmaps(this.DeviceTexture);
        }
        this._isDirty = false;
    }

    public virtual void Dispose()
    {
        this.DeviceTexture.Dispose();
    }
}