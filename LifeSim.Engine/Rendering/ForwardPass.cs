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
            public Matrix4x4 ShadowMapMatrix0 { get; set; }
            public Matrix4x4 ShadowMapMatrix1 { get; set; }
            public Matrix4x4 ShadowMapMatrix2 { get; set; }
            public Matrix4x4 ShadowMapMatrix3 { get; set; }

            // x = depth bias, y = normal bias, z = unused, w = unused (x4 cascades)
            public Vector4 ShadowBiasData0 { get; set; }
            public Vector4 ShadowBiasData1 { get; set; }
            public Vector4 ShadowBiasData2 { get; set; }
            public Vector4 ShadowBiasData3 { get; set; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct LightInfo
        {
            public ColorF AmbientColor { get; set; }
            public ColorF MainLightColor { get; set; }
            public Vector3 MainLightDirection { get; set; }
            private readonly float _padding0;
            public Vector4 ShadowMapDistances { get; set; } // Each component is the far plane of a cascade
        }

        private readonly GraphicsDevice _gd;
        private readonly DeviceBuffer _lightInfoBuffer;
        private readonly DeviceBuffer _camera3DInfoBuffer;
        private readonly ResourceLayout _resourceLayout;
        private readonly ResourceSet _resourceSet;
        private readonly IRenderTexture _renderTexture;
        private readonly SceneStorage _storage;
        private readonly RenderJob _renderJob;
        private readonly RenderQueue _renderQueue;

        private readonly ShadowPass _shadowPass;

        public ForwardPass(GraphicsDevice gd, SceneStorage storage, IRenderTexture mainRenderTexture, ShadowPass shadowPass)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._storage = storage;
            this._shadowPass = shadowPass;

            this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("LightDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));

            this._camera3DInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<CameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            this._lightInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout,
                this._camera3DInfoBuffer,
                this._lightInfoBuffer,
                shadowPass.ShadowmapTexture.DeviceTexture,
                shadowPass.ShadowmapTexture.ShadowSampler
            ));

            this._renderTexture = mainRenderTexture;

            this._renderJob = new RenderJob(this._gd, this._resourceSet, false);

            this._renderQueue = new RenderQueue();
        }

        public void Render(
            CommandList commandList,
            IReadOnlyList<Renderable> renderList,
            ICamera camera,
            Vector3 mainLightDirection,
            ColorF mainLightColor,
            ColorF ambientColor
        )
        {
            this._renderQueue.AddToRenderQueue(renderList, camera.FrustumForCulling, camera.Position);
            this._renderQueue.Sort();

            commandList.SetFramebuffer(this._renderTexture.Framebuffer);

            if (camera.ClearColor != null)
            {
                ColorF clearColor = camera.ClearColor.Value;
                commandList.ClearColorTarget(0, new RgbaFloat(clearColor.R, clearColor.G, clearColor.B, clearColor.A));
                commandList.ClearColorTarget(1, RgbaFloat.Black);
                commandList.ClearDepthStencil(1f);
            }

            CameraInfo cameraInfo = new CameraInfo();
            cameraInfo.ViewProjectionMatrix = camera.ViewProjectionMatrix;
            cameraInfo.ShadowMapMatrix0 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(0);
            cameraInfo.ShadowMapMatrix1 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(1);
            cameraInfo.ShadowMapMatrix2 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(2);
            cameraInfo.ShadowMapMatrix3 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(3);
            cameraInfo.ShadowBiasData0 = this._shadowPass.GetShadowBiasData(0);
            cameraInfo.ShadowBiasData1 = this._shadowPass.GetShadowBiasData(1);
            cameraInfo.ShadowBiasData2 = this._shadowPass.GetShadowBiasData(2);
            cameraInfo.ShadowBiasData3 = this._shadowPass.GetShadowBiasData(3);

            LightInfo lightInfo = new LightInfo();
            lightInfo.AmbientColor = ambientColor;
            lightInfo.MainLightColor = mainLightColor;
            lightInfo.MainLightDirection = Vector3.Normalize(mainLightDirection);
            lightInfo.ShadowMapDistances = this._shadowPass.GetShadowCascadeDistances();

            commandList.UpdateBuffer(this._camera3DInfoBuffer, 0, ref cameraInfo);
            commandList.UpdateBuffer(this._lightInfoBuffer, 0, ref lightInfo);

            this._renderJob.DrawRenderList(commandList, this._renderQueue);
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