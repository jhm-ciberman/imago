using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class PassManager
    {
        public readonly Pass color;
        public readonly Pass shadowMap;
        public readonly Pass fullscreen;
        public readonly Pass sprites;

        public PassManager(GraphicsDevice gd, GPUResourceManager resources)
        {
            var sceneContext = resources.sceneContext;
            var shadowmapTexture = sceneContext.shadowmapTexture;


            var shadowMapSampler = gd.ResourceFactory.CreateSampler(new SamplerDescription (
                SamplerAddressMode.Border, SamplerAddressMode.Border, SamplerAddressMode.Border,
                SamplerFilter.MinLinear_MagLinear_MipLinear, null, 0, 0, 0, 0, SamplerBorderColor.OpaqueWhite
            ));

            IMaterialBuilder materialBuilder = resources;
            var passes = materialBuilder.layouts.passes;
            var colorPassResources     = new ResourceSetDescription(passes.color, sceneContext.camera3DInfoBuffer, sceneContext.lightInfoBuffer, shadowmapTexture, shadowMapSampler);
            var shadowmapPassResources = new ResourceSetDescription(passes.shadowMap, sceneContext.shadowmapInfoBuffer);
            var spritesPassResources   = new ResourceSetDescription(passes.sprites, sceneContext.camera2DInfoBuffer);

            this.color = new Pass(gd, "base", colorPassResources, new Pass.Description {
                blendState = new BlendStateDescription(RgbaFloat.Black, BlendAttachmentDescription.OverrideBlend, BlendAttachmentDescription.Disabled),
                faceCullMode = FaceCullMode.Front,
                outputDescription = resources.mainRenderTexture.outputDescription,
            });

            this.shadowMap = new Pass(gd, "shadowmap", shadowmapPassResources, new Pass.Description {
                blendState = BlendStateDescription.Empty,
                faceCullMode = FaceCullMode.Front,
                outputDescription = sceneContext.shadowmapFramebuffer.OutputDescription,
            });

            this.sprites = new Pass(gd, "sprites", spritesPassResources, new Pass.Description {
                blendState = BlendStateDescription.SingleAlphaBlend,
                faceCullMode = FaceCullMode.None,
                outputDescription = resources.mainRenderTexture.outputDescription,
            });

            this.fullscreen = new Pass(gd, "fullscreen", null, new Pass.Description {
                blendState = BlendStateDescription.SingleOverrideBlend,
                faceCullMode = FaceCullMode.None,
                outputDescription = resources.fullScreenRenderTexture.outputDescription,
            });
        }
    }
}