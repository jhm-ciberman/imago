using System;
using System.Numerics;
using System.Threading.Tasks;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class ShadowMapTexture : ITexture, IDisposable
    {
        public uint Width { get; private set; }

        public uint Height { get; private set; }

        public uint ArrayLayers { get; private set; }

        public Veldrid.Texture DeviceTexture { get; private set; }

        public Sampler Sampler { get; private set; }

        public Framebuffer[] Framebuffers { get; private set; }

        public ShadowMapTexture(GraphicsDevice gd, uint width, uint height, uint arrayLayers)
        {
            this.Width = width;
            this.Height = height;
            this.ArrayLayers = arrayLayers;
            this.DeviceTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(this.Width, this.Height, 1, this.ArrayLayers, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
            this.Sampler = gd.LinearSampler;

            this.Framebuffers = new Framebuffer[this.ArrayLayers];

            for (uint i = 0; i < this.ArrayLayers; i++)
            {
                this.Framebuffers[i] = gd.ResourceFactory.CreateFramebuffer(new FramebufferDescription(
                    new FramebufferAttachmentDescription(this.DeviceTexture, i),
                    Array.Empty<FramebufferAttachmentDescription>()
                ));
            }
        }

        public void Dispose()
        {
            this.DeviceTexture.Dispose();

            foreach (Framebuffer fb in this.Framebuffers)
            {
                fb.Dispose();
            }
        }
    }
}