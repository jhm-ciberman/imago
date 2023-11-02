using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Rendering.Passes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = global::Support.Color;
using Veldrid;
using Vector2Int = Support.Vector2Int;

namespace Imago.Rendering;

/// <summary>
/// Represents a texture.
/// </summary>
public class Texture : ITexture, ITextureRegion, IDisposable
{
    private static Texture? _white = null;
    private static Texture? _black = null;
    private static Texture? _transparent = null;
    private static Texture? _magenta = null;


    /// <summary>
    /// Gets a small white texture.
    /// </summary>
    public static Texture White => _white ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(255, 255, 255, 255)));

    /// <summary>
    /// Gets a small black texture.
    /// </summary>
    public static Texture Black => _black ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(0, 0, 0, 255)));

    /// <summary>
    /// Gets a small transparent texture.
    /// </summary>
    public static Texture Transparent => _transparent ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(0, 0, 0, 0)));

    /// <summary>
    /// Gets a small magenta texture.
    /// </summary>
    public static Texture Magenta => _magenta ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(255, 0, 255, 255)));

    /// <summary>
    /// Occurs when the texture is resized.
    /// </summary>
    public event EventHandler? Resized;

    /// <summary>
    /// Gets the Veldrid texture object.
    /// </summary>
    public Veldrid.Texture VeldridTexture { get; protected set; }

    /// <summary>
    /// Gets the Veldrid sampler object.
    /// </summary>
    public Sampler VeldridSampler { get; private set; }

    /// <summary>
    /// Gets the width of the texture.
    /// </summary>
    public uint Width { get; protected set; }

    /// <summary>
    /// Gets the height of the texture.
    /// </summary>
    public uint Height { get; protected set; }

    /// <summary>
    /// Gets the number of mip levels.
    /// </summary>
    public uint MipLevels { get; protected set; }

    private byte[] _data;

    private bool _isDirty = false;

    private readonly Renderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Texture"/> class.
    /// </summary>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <param name="mipLevels">The number of mip levels.</param>
    /// <param name="srgb">Whether to use sRGB.</param>
    public Texture(uint width, uint height, uint mipLevels = 0, bool srgb = true)
    {
        this._renderer = Renderer.Instance;
        this.Width = width;
        this.Height = height;

        this.MipLevels = (mipLevels == 0)
            ? (uint)BitOperations.Log2(Math.Min(width, height))
            : mipLevels;

        this._data = new byte[width * height * 4];
        PixelFormat pixelFormat = srgb ? PixelFormat.R8_G8_B8_A8_UNorm_SRgb : PixelFormat.R8_G8_B8_A8_UNorm;

        var gd = this._renderer.GraphicsDevice;
        this.VeldridTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
            this.Width, this.Height, this.MipLevels, 1,
            pixelFormat, TextureUsage.Sampled | TextureUsage.GenerateMipmaps
        ));
        this.VeldridSampler = gd.PointSampler;
        this.OnTextureDirty();
    }

    /// <summary>
    /// Gets the ImGui binding for this texture.
    /// </summary>
    public IntPtr ImGuiBinding => ImGuiPass.Instance.GetOrCreateBinding(this);  // TODO: Texture should not know about ImGuiPass

    /// <summary>
    /// Gets the size of the texture.
    /// </summary>
    public Vector2Int Size => new Vector2Int((int)this.Width, (int)this.Height);

    /// <summary>
    /// Gets or sets the name of the texture.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Called when the texture is dirty.
    /// </summary>
    protected void OnTextureDirty()
    {
        if (this._isDirty) return;

        this._isDirty = true;
        this._renderer.NotifyTextureDirty(this);
    }

    /// <summary>
    /// Fills the texture with a color.
    /// </summary>
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

    /// <summary>
    /// Sets the texture data from an image.
    /// </summary>
    /// <param name="image">The image.</param>
    public unsafe void SetDataFromImage(Image<Rgba32> image)
    {
        if (image.Width != this.Width || image.Height != this.Height)
        {
            throw new ArgumentException("Image size does not match texture size.");
        }

        if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixels))
        {
            throw new ArgumentException("Image is not in RGBA32 format.");
        }

        var dest = new Span<byte>(this._data);
        MemoryMarshal.Cast<Rgba32, byte>(pixels).CopyTo(dest);

        this.OnTextureDirty();
    }

    /// <summary>
    /// Sets the texture data from a byte array of RGBA data.
    /// </summary>
    /// <param name="data">The data.</param>
    /// <param name="x">The x coordinate to start at.</param>
    /// <param name="y">The y coordinate to start at.</param>
    /// <param name="width">The width of the data.</param>
    /// <param name="height">The height of the data.</param>
    /// <exception cref="ArgumentException">Texture size does not match data size.</exception>
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

    /// <summary>
    /// Updates the texture on the GPU.
    /// </summary>
    /// <param name="gd">The graphics device.</param>
    /// <param name="cl">The command list.</param>
    public void Update(GraphicsDevice gd, CommandList cl)
    {
        if (!this._isDirty) return;
        this._isDirty = false;

        if (this.Width != this.VeldridTexture.Width || this.Height != this.VeldridTexture.Height)
        {
            this.VeldridTexture.Dispose();
            this.VeldridTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                this.Width, this.Height, this.MipLevels, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled | TextureUsage.GenerateMipmaps
            ));
        }

        gd.UpdateTexture(this.VeldridTexture, this._data,
            x: 0, y: 0, z: 0,
            width: (uint)this.Width, height: (uint)this.Height, depth: 1,
            arrayLayer: 0, mipLevel: 0);

        if (this.MipLevels > 1)
        {
            cl.GenerateMipmaps(this.VeldridTexture);
        }
    }

    /// <summary>
    /// Resizes the texture.
    /// </summary>
    /// <param name="width">The new width.</param>
    /// <param name="height">The new height.</param>
    public void Resize(uint width, uint height)
    {
        this.Width = width;
        this.Height = height;
        this._data = new byte[width * height * 4];
        this.OnTextureDirty();
        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Disposes the resources used by the texture.
    /// </summary>
    public virtual void Dispose()
    {
        Renderer.Instance.DisposeWhenIdle(this.VeldridTexture);
    }

    // ITextureRegion implementation for the whole texture
    Texture ITextureRegion.Texture => this;
    Vector2 ITextureRegion.TopLeft => Vector2.Zero;
    Vector2 ITextureRegion.BottomRight => Vector2.One;
}
