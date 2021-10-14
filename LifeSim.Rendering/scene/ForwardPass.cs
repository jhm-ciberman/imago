using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Rendering
{
    public class ForwardPass : IDisposable, IPass
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
        private readonly SceneRenderer _renderer;
        private readonly RenderQueue _renderQueue;
        private readonly RenderJob _renderJob;
        public ForwardPass(GraphicsDevice gd, SceneRenderer renderer, IRenderTexture mainRenderTexture, Veldrid.Texture shadowmapTexture)
        {
            this._gd = gd;
            var factory = gd.ResourceFactory;
            this._renderer = renderer;

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
                SamplerFilter.MinLinear_MagLinear_MipPoint, null, 0, 0, 0, 0, SamplerBorderColor.OpaqueWhite
            ));

            this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._camera3DInfoBuffer, this._lightInfoBuffer, shadowmapTexture, shadowMapSampler));
            this._shadowMapScaling = (this._gd.IsUvOriginTopLeft)
                ? Matrix4x4.CreateScale(.5f, -.5f, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f)
                : Matrix4x4.CreateScale(.5f, .5f, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f);

            this._renderTexture = mainRenderTexture;

            this._renderJob = new RenderJob(this._gd, this._resourceSet, false);
            this._renderQueue = new RenderQueue();
        }

        public void Render(
            CommandList commandList,
            IReadOnlyList<Renderable> renderables,
            DirectionalLight mainLight,
            ColorF ambientColor,
            ColorF clearColor,
            ICamera camera
        )
        {
            var cameraFrustum = camera.FrustumForCulling;
            this._renderQueue.AddToRenderQueue(renderables, ref cameraFrustum, camera.Position);
            this._renderQueue.Sort();

            commandList.SetFramebuffer(this._renderTexture.Framebuffer);
            commandList.ClearColorTarget(0, new RgbaFloat(clearColor.R, clearColor.G, clearColor.B, clearColor.A));
            commandList.ClearColorTarget(1, RgbaFloat.Black);
            commandList.ClearDepthStencil(1f);

            CameraInfo cameraInfo = new CameraInfo();
            cameraInfo.ViewProjectionMatrix = camera.ViewProjectionMatrix;
            cameraInfo.ShadowMapMatrix = mainLight.GetShadowMapMatrix(camera.Position) * this._shadowMapScaling;

            LightInfo lightInfo = new LightInfo();
            lightInfo.AmbientColor = ambientColor;
            lightInfo.MainLightColor = mainLight.Color;
            lightInfo.MainLightDirection = Vector3.Normalize(mainLight.Direction);

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
                BlendState = new BlendStateDescription(
                    RgbaFloat.Black,
                    BlendAttachmentDescription.OverrideBlend,
                    BlendAttachmentDescription.Disabled
                ),
                RasterizerState = rasterizerState,
                Outputs = this._renderTexture.OutputDescription,
                ResourceLayouts = this._renderer.GetShaderVariantResourceLayouts(this._resourceLayout, shaderVariant),
            });
        }
    }
}