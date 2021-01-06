using Veldrid;

namespace LifeSim.Rendering
{
    public class GPUResources : IMaterialBuilder 
    {
        private GraphicsDevice _gd;

        private ResourceLayouts _layouts;

        private PassManager _passes;

        private SceneContext _sceneContext;

        public readonly IRenderTexture fullScreenRenderTexture;

        public readonly RenderTexture mainRenderTexture;

        public GPUResources(GraphicsDevice gd, uint width, uint height)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._layouts = new ResourceLayouts(factory);
            this._sceneContext = new SceneContext(this._layouts, this._gd);

            this.fullScreenRenderTexture = new SwapchainRenderTexture(this._gd.MainSwapchain);
            this.mainRenderTexture = new RenderTexture(factory, width, height);

            this._passes = new PassManager(this._gd, this);
        }

        ResourceLayouts IMaterialBuilder.layouts => this._layouts;

        PassManager IMaterialBuilder.passes => this._passes;

        public SceneContext sceneContext => this._sceneContext;

        Sampler IMaterialBuilder.linearSampler => this._gd.LinearSampler;

        Sampler IMaterialBuilder.pointSampler => this._gd.PointSampler;

        ResourceSet IMaterialBuilder.CreateResourceSet(IMaterial material, params BindableResource[] resources)
        {
            return this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(material.resourceLayout, resources));
        }
    }
}