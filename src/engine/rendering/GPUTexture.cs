using System;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid.ImageSharp;

namespace LifeSim.Engine.Rendering
{
    public class GPUTexture : System.IDisposable
    {
        protected Veldrid.Texture _deviceTexture;
        protected Veldrid.Sampler _sampler;
        protected Veldrid.GraphicsDevice _gd;

        public GPUTexture(Veldrid.GraphicsDevice gd, ImageSharpTexture texture)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            var deviceTexture = texture.CreateDeviceTexture(this._gd, factory);

            this._deviceTexture = deviceTexture;
            this._sampler = this._gd.PointSampler;
        }

        public GPUTexture(Veldrid.GraphicsDevice gd, uint width, uint height)
        {
            this._gd = gd;
            var factory = this._gd.ResourceFactory;
            this._deviceTexture = factory.CreateTexture(new Veldrid.TextureDescription(
                (uint) width, (uint) height, depth: 1, 
                mipLevels: 1, arrayLayers: 1, 
                Veldrid.PixelFormat.R8_G8_B8_A8_UNorm, 
                Veldrid.TextureUsage.Sampled, 
                Veldrid.TextureType.Texture2D
            ));
            this._sampler = this._gd.PointSampler;
        }

        public uint width => this._deviceTexture.Width;
        public uint height => this._deviceTexture.Height;

        public Veldrid.Texture deviceTexture => this._deviceTexture;
        public Veldrid.Sampler sampler => this._sampler;

        void Update(RectInt bounds, byte[] data)
        {
            this._gd.UpdateTexture(
                this._deviceTexture, data, 
                x: (uint) bounds.x, y: (uint) bounds.y, z: 0, 
                width: (uint) bounds.width, height: (uint) bounds.height, depth: 1, 
                mipLevel: 0, arrayLayer: 0
            );
        }

        public unsafe void Update(Image<Rgba32> image)
        {
            if (!image.TryGetSinglePixelSpan(out Span<Rgba32> pixelSpan)) {
                throw new System.Exception("Unable to get image pixelspan.");
            }

            fixed (void* pin = &MemoryMarshal.GetReference(pixelSpan)) { // TODO: Generate mipmaps, check matching width/height, etc
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
        }

        public void Dispose()
        {
            this._deviceTexture.Dispose();
        }

    }
}