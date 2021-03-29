using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class GPUResourceManager : IMaterialBuilder 
    {
        private readonly GraphicsDevice _gd;

        private readonly ResourceLayouts _layouts;

        private readonly PassManager _passes;

        private readonly SceneManager _sceneManager;

        public readonly IRenderTexture fullScreenRenderTexture;

        public readonly RenderTexture mainRenderTexture;

        public GPUResourceManager(GraphicsDevice gd, uint width, uint height)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._layouts = new ResourceLayouts(factory);
            this._sceneManager = new SceneManager(this._layouts, this._gd);

            this.fullScreenRenderTexture = new SwapchainRenderTexture(this._gd.MainSwapchain);
            this.mainRenderTexture = new RenderTexture(factory, width, height);

            this._passes = new PassManager(this._gd, this);
        }

        ResourceLayouts IMaterialBuilder.layouts => this._layouts;

        PassManager IMaterialBuilder.passes => this._passes;

        public SceneManager sceneManager => this._sceneManager;

        Sampler IMaterialBuilder.linearSampler => this._gd.LinearSampler;

        Sampler IMaterialBuilder.pointSampler => this._gd.PointSampler;

        ResourceSet IMaterialBuilder.CreateResourceSet(IMaterial material, params BindableResource[] resources)
        {
            return this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(material.resourceLayout, resources));
        }

        private GPUTexture? _cachedPinkTexture = null;
        public GPUTexture pinkTexture 
        {
            get
            {
                if (this._cachedPinkTexture != null) return this._cachedPinkTexture;
                
                Image<Rgba32> image = new Image<Rgba32>(2, 2, new Rgba32(255, 0, 255));
                return this._cachedPinkTexture = new GPUTexture(this._gd, image);
            }
        }
    }
}