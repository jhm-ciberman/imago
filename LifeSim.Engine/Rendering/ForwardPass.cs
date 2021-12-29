using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class ForwardPass : IDisposable, IPipelineProvider
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct CameraInfo
        {
            public Matrix4x4 ViewProjectionMatrix { get; set; }
            public Matrix4x4 ShadowMapMatrix { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct LightInfo
        {
            public ColorF AmbientColor { get; set; }
            public ColorF MainLightColor { get; set; }
            public Vector3 MainLightDirection { get; set; }
            private readonly float _padding0;
        }

        private readonly GraphicsDevice _gd;
        private readonly DeviceBuffer _lightInfoBuffer;
        private readonly DeviceBuffer _camera3DInfoBuffer;
        private readonly ResourceLayout _resourceLayout;
        private readonly ResourceSet _resourceSet;
        private readonly Matrix4x4 _shadowMapScaling;
        private readonly IRenderTexture _renderTexture;
        private readonly SceneStorage _storage;
        private readonly RenderJob _renderJob;
        public ForwardPass(GraphicsDevice gd, SceneStorage storage, IRenderTexture mainRenderTexture, ShadowPass shadowPass)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._storage = storage;

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("LightDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            this._camera3DInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this._lightInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            var shadowMapSampler = factory.CreateSampler(new SamplerDescription (
                SamplerAddressMode.Border, SamplerAddressMode.Border, SamplerAddressMode.Border,
                SamplerFilter.MinPoint_MagPoint_MipPoint, null, 0, 0, 0, 0, SamplerBorderColor.OpaqueWhite
            ));

            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout,
                this._camera3DInfoBuffer,
                this._lightInfoBuffer,
                shadowPass.ShadowmapTexture.DeviceTexture,
                shadowMapSampler
            ));

            this._shadowMapScaling = (this._gd.IsUvOriginTopLeft)
                ? Matrix4x4.CreateScale(.5f, -.5f, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f)
                : Matrix4x4.CreateScale(.5f, .5f, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f);

            this._renderTexture = mainRenderTexture;

            this._renderJob = new RenderJob(this._gd, this._resourceSet, false);
        }

        public void Render(
            CommandList commandList,
            IReadOnlyList<Renderable> renderQueue,
            ICamera camera,
            DirectionalLight mainLight,
            ColorF ambientColor
        )
        {
            commandList.SetFramebuffer(this._renderTexture.Framebuffer);

            CameraInfo cameraInfo = new CameraInfo();
            cameraInfo.ViewProjectionMatrix = camera.ViewProjectionMatrix;
            cameraInfo.ShadowMapMatrix = mainLight.GetShadowMapMatrix(camera) * this._shadowMapScaling;

            LightInfo lightInfo = new LightInfo();
            lightInfo.AmbientColor = ambientColor;
            lightInfo.MainLightColor = mainLight.Color;
            lightInfo.MainLightDirection = Vector3.Normalize(mainLight.Direction);

            commandList.UpdateBuffer(this._camera3DInfoBuffer, 0, ref cameraInfo);
            commandList.UpdateBuffer(this._lightInfoBuffer, 0, ref lightInfo);

            this._renderJob.DrawRenderList(commandList, renderQueue);
        }

        public void Dispose()
        {
            this._resourceLayout.Dispose();
            this._resourceSet.Dispose();
            this._camera3DInfoBuffer.Dispose();
            this._lightInfoBuffer.Dispose();
        }

        Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.Front,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            );

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
            {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleList,
                ShaderSet = shaderVariant.ShaderSetDescription,
                BlendState = new BlendStateDescription(
                    RgbaFloat.Black,
                    BlendAttachmentDescription.OverrideBlend,
                    BlendAttachmentDescription.Disabled
                ),
                RasterizerState = rasterizerState,
                Outputs = this._renderTexture.OutputDescription,
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