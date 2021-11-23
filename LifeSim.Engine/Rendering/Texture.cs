using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LifeSim.Engine.Rendering
{
    public class Texture
    {
        public Veldrid.Texture DeviceTexture { get; protected set; }
        public Sampler Sampler { get; private set; }
        private bool _isDirty = true;
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int MipLevels { get; protected set; }

        private static Texture? _whiteTexture = null;
        private static Texture? _blackTexture = null;
        private static Texture? _pinkTexture = null;
        public static Texture White => _whiteTexture ??= new Texture(new Image<Rgba32>(4, 4, new Rgba32(255, 255, 255, 255)));
        public static Texture Black => _blackTexture ??= new Texture(new Image<Rgba32>(4, 4, new Rgba32(0, 0, 0, 255)));
        public static Texture Pink => _pinkTexture ??= new Texture(new Image<Rgba32>(4, 4, new Rgba32(255, 0, 255, 255)));

        private readonly Image<Rgba32> _image;


        public Texture(string path, int mipLevels = 0)
            : this(Image.Load<Rgba32>(path), mipLevels)
        {
            //
        }

        public Texture(Image<Rgba32> image, int mipLevels = 0)
            : this(image, image.Width, image.Height, mipLevels)
        {
            //
        }

        public Texture(int width, int height, int mipLevels = 0)
            : this(new Image<Rgba32>(width, height), mipLevels)
        {
            //
        }

        public Texture(Image<Rgba32> image, int width, int height, int mipLevels = 0)
        {
            this._image = image;
            this.Width = width;
            this.Height = height;

            this.MipLevels = (mipLevels == 0)
                ? BitOperations.Log2((uint)Math.Min(width, height))
                : mipLevels;

            var gd = Renderer.GraphicsDevice;
            this.DeviceTexture = this.CreateDeviceTexture(gd);
            this.Sampler = gd.PointSampler;
            this._isDirty = true;
            Renderer.Instance.OnTextureDirty(this);
        }

        private Veldrid.Texture CreateDeviceTexture(GraphicsDevice gd)
        {
            return gd.ResourceFactory.CreateTexture(new TextureDescription(
                (uint)this.Width, (uint)this.Height, 1,
                (uint)this.MipLevels, 1,
                PixelFormat.R8_G8_B8_A8_UNorm,
                TextureUsage.Sampled | TextureUsage.GenerateMipmaps,
                TextureType.Texture2D
            ));
        }

        private void _OnTextureChanged()
        {
            if (!this._isDirty)
            {
                this._isDirty = true;
                Renderer.Instance.OnTextureDirty(this);
            }
        }

        public void Apply()
        {
            this._OnTextureChanged();
        }

        public void Dispose()
        {
            this.DeviceTexture.Dispose();
        }

        public void SetData(int x, int y, int width, int height, byte[] data)
        {
            Debug.Assert(x >= 0 && y >= 0 && x + width <= this.Width && y + height <= this.Height);

            // Copy the data bytes to the image
            int index = 0;
            for (int yi = y; yi < y + height; yi++)
            {
                Span<Rgba32> row = this._image.GetPixelRowSpan(yi);
                for (int xi = 0; xi < width; xi++)
                {
                    row[x + xi] = new Rgba32(data[index + 0], data[index + 1], data[index + 2], data[index + 3]);
                    index += 4;
                }
            }

            this._OnTextureChanged();
        }


        public unsafe void Update(GraphicsDevice gd, CommandList cl)
        {
            if (!this._image.TryGetSinglePixelSpan(out Span<Rgba32> pixelSpan))
            {
                throw new Exception("Unable to get image pixelspan.");
            }

            fixed (void* pin = &MemoryMarshal.GetReference(pixelSpan))
            {
                gd.UpdateTexture(
                    this.DeviceTexture,
                    (IntPtr)pin,
                    (uint)(sizeof(byte) * 4 * this.Width * this.Height),
                    x: 0, y: 0, z: 0,
                    width: (uint)this.Width, height: (uint)this.Height, depth: 1,
                    mipLevel: 0, arrayLayer: 0
                );
            }

            if (this.MipLevels > 0)
            {
                cl.GenerateMipmaps(this.DeviceTexture);
            }

            this._isDirty = false;
        }
    }
}