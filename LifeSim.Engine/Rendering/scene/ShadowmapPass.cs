using System;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public class ShadowmapPass : IDisposable, PipelineCache.IPipelineFactory
    {
        public Veldrid.Texture shadowmapTexture { get; private set; }

        public ResourceLayout _resourceLayout;
        public ResourceSet _resourceSet;

        private GraphicsDevice _gd;

        private readonly Framebuffer _shadowmapFramebuffer;
        public readonly DeviceBuffer _shadowmapInfoBuffer;

        private RenderQueue _renderQueue;
        private RenderJob _renderJob;
        private SceneRenderer _renderer;

        public ShadowmapPass(GraphicsDevice gd, SceneRenderer renderer)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._renderer = renderer;

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ShadowMapInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            uint shadowMapSize = 4096;
            this.shadowmapTexture = factory.CreateTexture(TextureDescription.Texture2D(shadowMapSize, shadowMapSize, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
            this._shadowmapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                this.shadowmapTexture, System.Array.Empty<Veldrid.Texture>()
            ));

            this._shadowmapInfoBuffer = factory.CreateBuffer(new BufferDescription((uint) Marshal.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));


            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._shadowmapInfoBuffer));

            this._renderQueue = new RenderQueue();
            this._renderJob = new RenderJob(this._resourceSet);
        }
        
        public void Render(CommandList commandList, Scene3D scene, Camera3D camera, DirectionalLight mainLight)
        {
            var matrix = mainLight.GetShadowMapMatrix(camera.position);
            var frustum = new BoundingFrustum(matrix);
            this._renderQueue.AddToRenderQueue(scene.storage, ref frustum, camera.position, true);
            this._renderQueue.Sort();

            var shadowmapMatrix = mainLight.GetShadowMapMatrix(camera.frustumCullingCamera.position);
            commandList.SetFramebuffer(this._shadowmapFramebuffer);
            commandList.ClearDepthStencil(1f);
            commandList.UpdateBuffer(this._shadowmapInfoBuffer, 0, ref shadowmapMatrix);

            this._renderJob.DrawRenderList(commandList, scene.storage.transformsResourceSet, this._renderQueue);
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
            this._resourceLayout.Dispose();
            this._shadowmapFramebuffer.Dispose();
            this._shadowmapInfoBuffer.Dispose();
        }

        Pipeline PipelineCache.IPipelineFactory.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.Front,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription() {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = shaderVariant.shaderSetDescription,
                BlendState = BlendStateDescription.Empty,
                RasterizerState = rasterizerState,
                Outputs = this._shadowmapFramebuffer.OutputDescription,
                ResourceLayouts = this._renderer.GetShaderVariantResourceLayouts(this._resourceLayout, shaderVariant),
            });
        }
    }
}