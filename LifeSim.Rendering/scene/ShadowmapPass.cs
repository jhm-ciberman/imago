using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Rendering
{
    public class ShadowmapPass : IDisposable, IPass
    {
        public Veldrid.Texture ShadowmapTexture { get; private set; }

        private readonly ResourceLayout _resourceLayout;
        private readonly ResourceSet _resourceSet;
        private readonly GraphicsDevice _gd;
        private readonly Framebuffer _shadowmapFramebuffer;
        private readonly DeviceBuffer _shadowmapInfoBuffer;
        private readonly RenderQueue _renderQueue;
        private readonly RenderJob _renderJob;
        private readonly SceneRenderer _renderer;

        public ShadowmapPass(GraphicsDevice gd, SceneRenderer renderer)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._renderer = renderer;

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ShadowMapDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            uint shadowMapSize = 4096;
            this.ShadowmapTexture = factory.CreateTexture(TextureDescription.Texture2D(shadowMapSize, shadowMapSize, 1, 1, PixelFormat.R32_Float, TextureUsage.DepthStencil | TextureUsage.Sampled));
            this._shadowmapFramebuffer = factory.CreateFramebuffer(new FramebufferDescription(
                this.ShadowmapTexture, System.Array.Empty<Veldrid.Texture>()
            ));

            this._shadowmapInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));


            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._shadowmapInfoBuffer));

            this._renderQueue = new RenderQueue();
            this._renderJob = new RenderJob(this._gd, this._resourceSet, true);
        }

        public void Render(CommandList commandList, IReadOnlyList<Renderable> renderables, ICamera camera, DirectionalLight mainLight)
        {
            var matrix = mainLight.GetShadowMapMatrix(camera.Position);
            var frustum = new BoundingFrustum(matrix);
            this._renderQueue.AddToRenderQueue(renderables, ref frustum, camera.Position);
            this._renderQueue.Sort();

            var shadowmapMatrix = mainLight.GetShadowMapMatrix(camera.Position);
            commandList.SetFramebuffer(this._shadowmapFramebuffer);
            commandList.ClearDepthStencil(1f);
            commandList.UpdateBuffer(this._shadowmapInfoBuffer, 0, ref shadowmapMatrix);

            this._renderJob.DrawRenderList(commandList, this._renderQueue);
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
            this._resourceLayout.Dispose();
            this._shadowmapFramebuffer.Dispose();
            this._shadowmapInfoBuffer.Dispose();
        }

        Pipeline IPass.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.Front,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
            {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = shaderVariant.ShaderSetDescription,
                BlendState = BlendStateDescription.Empty,
                RasterizerState = rasterizerState,
                Outputs = this._shadowmapFramebuffer.OutputDescription,
                ResourceLayouts = this._renderer.GetShaderVariantResourceLayouts(this._resourceLayout, shaderVariant),
            });
        }
    }
}