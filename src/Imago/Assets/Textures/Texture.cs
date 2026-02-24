using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Color = Imago.Support.Drawing.Color;
using Vector2Int = Imago.Support.Numerics.Vector2Int;

namespace Imago.Assets.Textures;

/// <summary>
/// Represents a 2D texture that can be used for rendering.
/// Wraps a Veldrid texture and provides methods for updating its content.
/// </summary>
public class Texture : ITexture, ITextureRegion, IDisposable
{
    private static Texture? _white = null;
    private static Texture? _black = null;
    private static Texture? _transparent = null;
    private static Texture? _magenta = null;


    /// <summary>
    /// Gets a singleton 2x2 white texture.
    /// </summary>
    public static Texture White => _white ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(255, 255, 255, 255)));

    /// <summary>
    /// Gets a singleton 2x2 black texture.
    /// </summary>
    public static Texture Black => _black ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(0, 0, 0, 255)));

    /// <summary>
    /// Gets a singleton 2x2 transparent texture.
    /// </summary>
    public static Texture Transparent => _transparent ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(0, 0, 0, 0)));

    /// <summary>
    /// Gets a singleton 2x2 magenta texture, often used to indicate missing textures.
    /// </summary>
    public static Texture Magenta => _magenta ??= new ImageTexture(new Image<Rgba32>(2, 2, new Rgba32(255, 0, 255, 255)));

    /// <summary>
    /// Occurs when the texture is resized.
    /// </summary>
    public event EventHandler? Resized;

    /// <summary>
    /// Gets the underlying Veldrid texture object.
    /// </summary>
    public Veldrid.Texture VeldridTexture { get; protected set; }

    /// <summary>
    /// Gets the Veldrid sampler object used for sampling this texture.
    /// </summary>
    public Sampler VeldridSampler { get; private set; }

    /// <summary>
    /// Gets the width of the texture in pixels.
    /// </summary>
    public uint Width { get; protected set; }

    /// <summary>
    /// Gets the height of the texture in pixels.
    /// </summary>
    public uint Height { get; protected set; }

    /// <summary>
    /// Gets the number of mipmap levels.
    /// </summary>
    public uint MipLevels { get; protected set; }

    private byte[] _data;

    private bool _isDirty = false;

    private readonly Renderer _renderer;

    /// <summary>
    /// Initializes a new instance of the <see cref="Texture"/> class.
    /// </summary>
    /// <param name="width">The width of the texture in pixels.</param>
    /// <param name="height">The height of the texture in pixels.</param>
    /// <param name="mipLevels">The number of mipmap levels. If 0, a full mipmap chain is generated.</param>
    /// <param name="srgb">A value indicating whether the texture uses the sRGB color space.</param>
    public Texture(uint width, uint height, uint mipLevels = 0, bool srgb = true)
    {
        this._renderer = Renderer.Instance;
        this.Width = width;
        this.Height = height;

        this.MipLevels = mipLevels == 0
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

        this._renderer.RegisterDisposable(this);
    }

    /// <summary>
    /// Gets the ImGui binding for this texture, allowing it to be displayed in an ImGui UI.
    /// </summary>
    public nint ImGuiBinding => Renderer.Instance.GetOrCreateImGuiBinding(this);

    /// <summary>
    /// Gets the size of the texture as a <see cref="Vector2Int"/>.
    /// </summary>
    public Vector2Int Size => new Vector2Int((int)this.Width, (int)this.Height);

    /// <summary>
    /// Gets or sets the name of the texture, used for debugging purposes.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this texture has been disposed.
    /// </summary>
    public bool IsDisposed => this.VeldridTexture.IsDisposed;

    /// <summary>
    /// Marks the texture as dirty, indicating that its data needs to be re-uploaded to the GPU.
    /// </summary>
    protected void OnTextureDirty()
    {
        if (this._isDirty) return;

        this._isDirty = true;
        this._renderer.NotifyTextureDirty(this);
    }

    /// <summary>
    /// Fills the entire texture with a single color.
    /// </summary>
    /// <param name="fillColor">The color to fill the texture with.</param>
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
    /// Sets the texture data from an <see cref="Image{Rgba32}"/>.
    /// </summary>
    /// <param name="image">The image to upload.</param>
    public unsafe void SetDataFromImage(Image<Rgba32> image)
    {
        if (image.Width != this.Width || image.Height != this.Height)
            throw new ArgumentException("Image size does not match texture size.");

        if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
            throw new InvalidOperationException("Image is not in RGBA32 format.");

        var span = memory.Span;

        var dest = new Span<byte>(this._data);
        MemoryMarshal.Cast<Rgba32, byte>(span).CopyTo(dest);

        this.OnTextureDirty();
    }

    /// <summary>
    /// Sets a region of the texture from a byte array of RGBA data.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left corner of the region to update.</param>
    /// <param name="y">The Y coordinate of the top-left corner of the region to update.</param>
    /// <param name="width">The width of the region to update.</param>
    /// <param name="height">The height of the region to update.</param>
    /// <param name="data">The raw RGBA pixel data.</param>
    public unsafe void SetDataFromBytes(int x, int y, int width, int height, byte[] data)
    {
        if (x + width > this.Width || y + height > this.Height)
            throw new ArgumentException("Texture size does not match data size.");

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
    /// Uploads the texture data to the GPU if it is dirty.
    /// </summary>
    /// <param name="cl">The command list to use for the upload.</param>
    public void Update(CommandList cl)
    {
        if (!this._isDirty) return;
        this._isDirty = false;

        var gd = Renderer.Instance.GraphicsDevice;
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
            width: this.Width, height: this.Height, depth: 1,
            arrayLayer: 0, mipLevel: 0);

        if (this.MipLevels > 1)
            cl.GenerateMipmaps(this.VeldridTexture);
    }

    /// <summary>
    /// Resizes the texture.
    /// </summary>
    /// <remarks>
    /// This operation discards the current texture content.
    /// </remarks>
    /// <param name="width">The new width in pixels.</param>
    /// <param name="height">The new height in pixels.</param>
    public void Resize(uint width, uint height)
    {
        this.Width = width;
        this.Height = height;
        this._data = new byte[width * height * 4];
        this.OnTextureDirty();
        this.Resized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Disposes the texture and releases its GPU resources.
    /// </summary>
    public virtual void Dispose()
    {
        if (this.IsDisposed) return;

        Renderer.Instance.DisposeWhenIdle(this.VeldridTexture);
        Renderer.Instance.UnregisterDisposable(this);
    }

    // ITextureRegion implementation for the whole texture
    Texture ITextureRegion.Texture => this;
    Vector2 ITextureRegion.TopLeft => Vector2.Zero;
    Vector2 ITextureRegion.BottomRight => Vector2.One;
}
