using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering
{
    public partial class ShadowPass : IDisposable, IPipelineProvider
    {
        public ShadowMapTexture ShadowmapTexture { get; private set; }

        private readonly ResourceLayout _resourceLayout;
        private readonly ResourceSet _resourceSet;
        private readonly GraphicsDevice _gd;
        private readonly DeviceBuffer _shadowmapInfoBuffer;
        private readonly RenderJob _renderJob;
        private readonly SceneStorage _storage;

        public ShadowPass(GraphicsDevice gd, SceneStorage storage)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._storage = storage;

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ShadowMapDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            this.ShadowmapTexture = new ShadowMapTexture(gd, 4096, 4096);


            this._shadowmapInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));


            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._shadowmapInfoBuffer));

            this._renderJob = new RenderJob(this._gd, this._resourceSet, true);
        }

        public void Render(CommandList commandList, IReadOnlyList<Renderable> renderQueue, ICamera camera, DirectionalLight mainLight)
        {
            var shadowmapMatrix = mainLight.GetShadowMapMatrix(camera.Position);
            commandList.SetFramebuffer(this.ShadowmapTexture.Framebuffer);
            commandList.ClearDepthStencil(1f);
            commandList.UpdateBuffer(this._shadowmapInfoBuffer, 0, ref shadowmapMatrix);

            this._renderJob.DrawRenderList(commandList, renderQueue);
        }

        public void Dispose()
        {
            this._resourceSet.Dispose();
            this._resourceLayout.Dispose();
            this.ShadowmapTexture.Dispose();
            this._shadowmapInfoBuffer.Dispose();
        }

        Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant)
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
                Outputs = this.ShadowmapTexture.OutputDescription,
                ResourceLayouts = this._GetResourceLayouts(shaderVariant),
            });
        }

        private ResourceLayout[] _GetResourceLayouts(ShaderVariant shaderVariant)
        {
            Debug.Assert(shaderVariant.MaterialResourceLayout != null);

            var resources = new List<ResourceLayout> {
                this._resourceLayout,
                this._storage.TransformResourceLayout,
                shaderVariant.MaterialResourceLayout,
                this._storage.InstanceResourceLayout
            };

            if (shaderVariant.VertexFormat.IsSkinned)
            {
                resources.Add(this._storage.SkeletonResourceLayout);
            }

            return resources.ToArray();
        }
    }
}