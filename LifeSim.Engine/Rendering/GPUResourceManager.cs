using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class GPUResourceManager : IMaterialBuilder 
    {
        public Pass colorPass { get; private set; }
        public Pass shadowMapPass { get; private set; }
        public Pass fullscreenPass { get; private set; }
        public Pass spritesPass { get; private set; }

        private readonly GraphicsDevice _gd;

        private readonly ShaderLayouts _layouts;

        private readonly SceneManager _sceneManager;

        public readonly IRenderTexture fullScreenRenderTexture;

        public readonly RenderTexture mainRenderTexture;

        public GPUResourceManager(GraphicsDevice gd, uint width, uint height)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._layouts = new ShaderLayouts(factory);
            this._sceneManager = new SceneManager(this._layouts, this._gd);

            this.fullScreenRenderTexture = new SwapchainRenderTexture(this._gd.MainSwapchain);
            this.mainRenderTexture = new RenderTexture(factory, width, height);

            var shadowMapSampler = factory.CreateSampler(new SamplerDescription (
                SamplerAddressMode.Border, SamplerAddressMode.Border, SamplerAddressMode.Border,
                SamplerFilter.MinLinear_MagLinear_MipPoint, null, 0, 0, 0, 0, SamplerBorderColor.OpaqueWhite
            ));

            var colorPassLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("LightInfo", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            var shadowmapPässLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ShadowMapInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            var spritesPassLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            var fullscreenPassLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                Array.Empty<ResourceLayoutElementDescription>()
            ));

            var ctx = this._sceneManager;
            var colorPassResources     = factory.CreateResourceSet(new ResourceSetDescription(colorPassLayout, ctx.camera3DInfoBuffer, ctx.lightInfoBuffer, ctx.shadowmapTexture, shadowMapSampler));
            var shadowmapPassResources = factory.CreateResourceSet(new ResourceSetDescription(shadowmapPässLayout, ctx.shadowmapInfoBuffer));
            var spritesPassResources   = factory.CreateResourceSet(new ResourceSetDescription(spritesPassLayout, ctx.camera2DInfoBuffer));
            var fullscreenPassResource = factory.CreateResourceSet(new ResourceSetDescription(fullscreenPassLayout));

            this.colorPass = new Pass("base", colorPassResources, colorPassLayout, new Pass.Description {
                blendState = new BlendStateDescription(RgbaFloat.Black, BlendAttachmentDescription.OverrideBlend, BlendAttachmentDescription.Disabled),
                faceCullMode = FaceCullMode.Front,
                outputDescription = this.mainRenderTexture.outputDescription,
            });

            this.shadowMapPass = new Pass("shadowmap", shadowmapPassResources, shadowmapPässLayout, new Pass.Description {
                blendState = BlendStateDescription.Empty,
                faceCullMode = FaceCullMode.Front,
                outputDescription = ctx.shadowmapFramebuffer.OutputDescription,
            });

            this.spritesPass = new Pass("sprites", spritesPassResources, spritesPassLayout, new Pass.Description {
                blendState = BlendStateDescription.SingleAlphaBlend,
                faceCullMode = FaceCullMode.None,
                outputDescription = this.mainRenderTexture.outputDescription,
            });

            this.fullscreenPass = new Pass("fullscreen", fullscreenPassResource, fullscreenPassLayout, new Pass.Description {
                blendState = BlendStateDescription.SingleOverrideBlend,
                faceCullMode = FaceCullMode.None,
                outputDescription = this.fullScreenRenderTexture.outputDescription,
            });
        }

        ShaderLayouts IMaterialBuilder.layouts => this._layouts;

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