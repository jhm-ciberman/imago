using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace LifeSim.Engine.Rendering
{
    public class Texture : ITexture
    {
        private static Texture? _whiteTexture = null;
        private static Texture? _blackTexture = null;
        private static Texture? _pinkTexture = null;
        public static Texture White => _whiteTexture ??= new Texture(4, 4, new Color(255, 255, 255, 255));
        public static Texture Black => _blackTexture ??= new Texture(4, 4, new Color(0, 0, 0, 255));
        public static Texture Pink => _pinkTexture ??= new Texture(4, 4, new Color(255, 0, 255, 255));


        public static Texture FromFile(string path)
        {
            using var img = Image.Load<Rgba32>(path);
            return FromImage(img);
        }

        public static Texture FromImage(Image<Rgba32> image)
        {
            var texture = new Texture((uint)image.Width, (uint)image.Height);
            texture.SetDataFromImage(image);
            return texture;
        }

        public Veldrid.Texture DeviceTexture { get; protected set; }
        public Sampler Sampler { get; private set; }
        public uint Width { get; protected set; }
        public uint Height { get; protected set; }
        public uint MipLevels { get; protected set; }

        private readonly byte[] _data;

        private bool _isDirty = false;

        protected readonly GraphicsDevice _gd;

        public Texture(uint width, uint height, uint mipLevels = 0)
        {
            this._gd = Renderer.Instance.GraphicsDevice;
            this.Width = width;
            this.Height = height;

            this.MipLevels = (mipLevels == 0)
                ? (uint)BitOperations.Log2(Math.Min(width, height))
                : mipLevels;

            this._data = new byte[width * height * 4];

            this.DeviceTexture = this._gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                this.Width, this.Height, this.MipLevels, 1,
                PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled | TextureUsage.GenerateMipmaps
            ));
            this.Sampler = this._gd.PointSampler;
            Renderer.Instance.OnTextureDirty(this);
        }

        protected Texture(uint width, uint height, Color fillColor)
            : this(width, height)
        {
            this.Fill(fillColor);
        }



        protected void OnTextureDirty()
        {
            if (!this._isDirty)
            {
                this._isDirty = true;
                Renderer.Instance.OnTextureDirty(this);
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
                throw new Exception("Failed to get pixel span");
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
}