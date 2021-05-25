using System.IO;
using System.Runtime.InteropServices;
using FontStashSharp;
using FontStashSharp.Interfaces;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;
using Veldrid.Utilities;
using Rectangle = System.Drawing.Rectangle;

namespace LifeSim.Engine
{
    public class ResourceFactory : ITexture2DCreator, IRenderingResourcesFactory
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
        private readonly SceneRenderer _sceneRenderer;

        public ResourceFactory(GraphicsDevice gd, SceneRenderer sceneRenderer)
        {
            this._gd = gd;
            this._sceneRenderer = sceneRenderer;
        }
        
        private SurfaceMaterial? _cachedDefaultSurfaceMaterial;
        public SurfaceMaterial defaultSurfaceMaterial
        {
            get
            {
                if (this._cachedDefaultSurfaceMaterial != null) {
                    return this._cachedDefaultSurfaceMaterial;
                }
                return this._cachedDefaultSurfaceMaterial = this._sceneRenderer.CreateSurfaceMaterial(this.pinkTexture);
            }
        }

        public Scene3D CreateScene()
        {
            return new Scene3D(this._sceneRenderer.sceneStorage);
        }

        public Rendering.Texture MakeTexture(string path, uint mipLevels = 0)
        {
            Image<Rgba32> image = Image.Load<Rgba32>(path);
            return new Rendering.Texture(this._gd, image, mipLevels);
        }

        public SurfaceMaterial MakeSurfaceMaterial(Rendering.Texture texture)
        {
            return this._sceneRenderer.CreateSurfaceMaterial(texture);
        }

        public Rendering.Texture MakeTexture(Image<Rgba32> image, uint mipLevels = 0)
        {
            return new Rendering.Texture(this._gd, image, mipLevels);
        }

        public GLTF.GLTFLoader LoadGLTF(string path, SurfaceMaterial? defaultMaterial = null)
        {
            return new GLTF.GLTFLoader(this, path, defaultMaterial);
        }

        Mesh IRenderingResourcesFactory.CreateMesh<T>(VertexFormat vertexFormat, T[] vertices, ushort[] indices, ref BoundingBox boundingBox)
        {
            uint vertexBufferSize = (uint) (Marshal.SizeOf<T>() * vertices.Length);
            DeviceBuffer vertexBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer));
            this._gd.UpdateBuffer<T>(vertexBuffer, 0, vertices);

            uint indexBufferSize = (uint) (sizeof(ushort) * indices.Length);
            DeviceBuffer indexBuffer = this._gd.ResourceFactory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer));
            this._gd.UpdateBuffer(indexBuffer, 0, indices);

            return new Mesh(vertexFormat, (uint) vertices.Length, (uint) indices.Length, ref boundingBox, vertexBuffer, indexBuffer);
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

        private Rendering.Texture? _cachedPinkTexture = null;
        public Rendering.Texture pinkTexture 
        {
            get
            {
                if (this._cachedPinkTexture != null) return this._cachedPinkTexture;
                
                Image<Rgba32> image = new Image<Rgba32>(2, 2, new Rgba32(255, 0, 255));
                return this._cachedPinkTexture = new Rendering.Texture(this._gd, image);
            }
        }

        ITexture2D ITexture2DCreator.Create(int width, int height)
        {
            return new FontTexture2D(this._gd, (uint) width, (uint) height);
        }
    }
}