using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Engine.Rendering
{
    public class Texture : System.IDisposable, FontStashSharp.Interfaces.ITexture2D
    {
        protected Veldrid.Texture _deviceTexture;
        protected Veldrid.Sampler _sampler;
        protected Veldrid.GraphicsDevice _gd;

        public Texture(string path, uint mipLevels = 0) 
            : this(Image.Load<Rgba32>(path), mipLevels)
        {
            //
        }

        public Texture(Image<Rgba32> image, uint mipLevels = 0)
            : this((uint) image.Width, (uint) image.Height, mipLevels)
        {
            this.Update(image, true);
        }

        public Texture(uint width, uint height, uint mipLevels = 0)
        {
            this._gd = Renderer.graphicsDevice;
            var factory = this._gd.ResourceFactory;
            if (mipLevels == 0) {
                mipLevels = (uint) BitOperations.Log2(Math.Min(width, height));
            }

            this._deviceTexture = factory.CreateTexture(new Veldrid.TextureDescription(
                (uint) width, (uint) height, depth: 1, 
                mipLevels: mipLevels, arrayLayers: 1, 
                Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, 
                Veldrid.TextureUsage.Sampled | Veldrid.TextureUsage.GenerateMipmaps, 
                Veldrid.TextureType.Texture2D
            ));
            
            this._sampler = this._gd.PointSampler;
        }

        public uint width => this._deviceTexture.Width;
        public uint height => this._deviceTexture.Height;

        public Veldrid.Texture deviceTexture => this._deviceTexture;
        public Veldrid.Sampler sampler => this._sampler;

        private void Update(uint x, uint y, uint width, uint height, byte[] data, bool generateMipmaps = true)
        {
            this._gd.UpdateTexture(
                this._deviceTexture, data, 
                x: x, y: y, z: 0, 
                width: width, height: height, depth: 1, 
                mipLevel: 0, arrayLayer: 0
            );

            if (generateMipmaps) this.RegenerateMipmaps();
        }

        public unsafe void Update(Image<Rgba32> image, bool generateMipmaps = true)
        {
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixelSpan)) {
                throw new System.Exception("Unable to get image pixelspan.");
            }

            fixed (void* pin = &MemoryMarshal.GetReference(pixelSpan)) { // TODO: check matching width/height, etc
                this._gd.UpdateTexture(
                    this._deviceTexture,
                    (IntPtr)pin,
                    (uint)(sizeof(byte) * 4 * image.Width * image.Height),
                    x: 0, y: 0, z: 0,
                    width: (uint) image.Width,
                    height: (uint) image.Height,
                    depth: 1, mipLevel: (uint) 0, arrayLayer: 0
                );
            }

            if (generateMipmaps) this.RegenerateMipmaps();
        }

        void FontStashSharp.Interfaces.ITexture2D.SetData(System.Drawing.Rectangle bounds, byte[] data)
        {
            this._gd.UpdateTexture(
                this._deviceTexture, data, 
                x: (uint) bounds.X, y: (uint) bounds.Y, z: 0, 
                width: (uint) bounds.Width, height: (uint) bounds.Height, depth: 1, 
                mipLevel: 0, arrayLayer: 0
            );
        }

        public void RegenerateMipmaps()
        {
            var cl = this._gd.ResourceFactory.CreateCommandList();
            cl.Begin();
            cl.GenerateMipmaps(this._deviceTexture);
            cl.End();

            this._gd.SubmitCommands(cl);
        }

        public void Dispose()
        {
            this._deviceTexture.Dispose();
        }

    }
}