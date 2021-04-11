using System.IO;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Rendering;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Rectangle = System.Drawing.Rectangle;

namespace LifeSim.Engine
{
    public class ResourceFactory : ITexture2DCreator
    {
        private class FontTexture2D : Rendering.Texture, ITexture2D
        {
            public FontTexture2D(GraphicsDevice gd, uint width, uint height) : base(gd, width, height)
            {
            }

            void ITexture2D.SetData(Rectangle bounds, byte[] data)
            {
                this._gd.UpdateTexture(
                    this._deviceTexture, data, 
                    x: (uint) bounds.X, y: (uint) bounds.Y, z: 0, 
                    width: (uint) bounds.Width, height: (uint) bounds.Height, depth: 1, 
                    mipLevel: 0, arrayLayer: 0
                );
            }
        }

        private readonly GraphicsDevice _gd;

        private readonly GPUResourceManager _gpuResourceManager;


        public ResourceFactory(GraphicsDevice gd, GPUResourceManager gpuResourceManager)
        {
            this._gd = gd;
            this._gpuResourceManager = gpuResourceManager;
        }
        
        private SurfaceMaterial? _cachedDefaultSurfaceMaterial;
        public SurfaceMaterial defaultSurfaceMaterial
        {
            get
            {
                if (this._cachedDefaultSurfaceMaterial != null) {
                    return this._cachedDefaultSurfaceMaterial;
                }
                return this._cachedDefaultSurfaceMaterial = new SurfaceMaterial(this._gpuResourceManager, this._gpuResourceManager.pinkTexture);
            }
        }

        public Rendering.Texture MakeTexture(string path, uint mipLevels = 0)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(path);
            return new Rendering.Texture(this._gd, image, mipLevels);
        }

        public Rendering.Texture MakeTexture(Image<Rgba32> image, uint mipLevels = 0)
        {
            return new Rendering.Texture(this._gd, image, mipLevels);
        }

        public GLTF.GLTFLoader LoadGLTF(string path, SurfaceMaterial? defaultMaterial = null)
        {
            return new GLTF.GLTFLoader(this, defaultMaterial, path);
        }

        public Mesh MakeMesh(MeshData meshData)
        {
            return new Mesh(this._gd, meshData);
        }

        public SurfaceMaterial MakeSurfaceMaterial(Rendering.Texture texture)
        {
            return this._gpuResourceManager.MakeSurfaceMaterial(texture);
        }

        public SpriteMaterial MakeSpritesMaterial(Veldrid.Texture texture)
        {
            return this._gpuResourceManager.MakeSpritesMaterial(texture);
        }

        public FontSystem MakeFontSystem(string[] paths)
        {
            var fontLoader = StbTrueTypeSharpFontLoader.Instance;
            int atlasSize = 1024;
            var fontSystem = new FontSystem(fontLoader, this, atlasSize, atlasSize, 0, 1, true);
            foreach (var path in paths) {
                fontSystem.AddFont(File.ReadAllBytes(path));
            }
            return fontSystem;
        }

        ITexture2D ITexture2DCreator.Create(int width, int height)
        {
            return new FontTexture2D(this._gd, (uint) width, (uint) height);
        }
    }
}