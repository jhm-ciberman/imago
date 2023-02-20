using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.SceneGraph;
using LifeSim.Support;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering.Passes;

internal class ForwardPass : IDisposable, IPipelineProvider, IRenderingPass
{
    [StructLayout(LayoutKind.Sequential)]
    private struct CameraDataBuffer
    {
        public Matrix4x4 ViewProjectionMatrix { get; set; }
        public Matrix4x4 ShadowMapMatrix0 { get; set; }
        public Matrix4x4 ShadowMapMatrix1 { get; set; }
        public Matrix4x4 ShadowMapMatrix2 { get; set; }
        public Matrix4x4 ShadowMapMatrix3 { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct LightInfo
    {
        public ColorF AmbientColor { get; set; } // rgb = color, a = intensity
        public ColorF MainLightColor { get; set; } // rgb = color, a = intensity
        public ColorF ShadowColor { get; set; } // rgb = color, a = intensity
        public Vector3 MainLightDirection { get; set; }
        private readonly float _padding0;
        public ColorF FogColor { get; set; } // rgb = color, a = unused
        public float FogStart { get; set; }
        public float FogEnd { get; set; }
        private readonly float _padding1;
        private readonly float _padding2;
        public Vector4 ShadowMapDistances { get; set; } // Each component is the far plane of a cascade
    }

    private readonly Renderer _renderer;
    private readonly GraphicsDevice _gd;
    private readonly DeviceBuffer _lightInfoBuffer;
    private readonly DeviceBuffer _camera3DInfoBuffer;
    private readonly ResourceLayout _resourceLayout;
    private ResourceSet _resourceSet;
    private readonly IRenderTexture _renderTexture;
    private readonly RenderJob _renderJob;
    private readonly RenderQueue _opaqueRenderQueue;
    private readonly RenderQueue _transparentRenderQueue;
    private readonly ShadowPass _shadowPass;

    public ForwardPass(Renderer renderer, IRenderTexture mainRenderTexture, ShadowPass shadowPass)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;
        this._shadowPass = shadowPass;

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("LightDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("ShadowMapTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        this._camera3DInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<CameraDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        this._lightInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._resourceSet = this.CreatePassResourceSet();

        this._renderTexture = mainRenderTexture;

        this._renderJob = new RenderJob(this._gd, false);

        this._opaqueRenderQueue = new RenderQueue(RenderQueues.Opaque);
        this._transparentRenderQueue = new RenderQueue(RenderQueues.Transparent);

        this._shadowPass.ShadowmapTexture.Resized += this.Shadowmap_Resized;
    }

    private void Shadowmap_Resized(object? sender, EventArgs e)
    {
        this._renderer.DisposeWhenIdle(this._resourceSet);
        this._resourceSet = this.CreatePassResourceSet();
    }

    private ResourceSet CreatePassResourceSet()
    {
        return this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout,
            this._camera3DInfoBuffer,
            this._lightInfoBuffer,
            this._shadowPass.ShadowmapTexture.VeldridTexture,
            this._shadowPass.ShadowmapTexture.ShadowSampler
        ));
    }

    public void Render(CommandList cl, Scene scene)
    {
        Camera3D? camera = scene.Camera;

        if (camera == null)
            return;

        var frustumForCulling = new BoundingFrustum(camera.FrustumCullingCamera.ViewProjectionMatrix);
        this._opaqueRenderQueue.Update(frustumForCulling, camera.Position);
        this._transparentRenderQueue.Update(frustumForCulling, camera.Position);

        cl.SetFramebuffer(this._renderTexture.Framebuffer);

        if (camera.ClearColor != null)
        {
            ColorF clearColor = camera.ClearColor.Value;
            cl.ClearColorTarget(0, new RgbaFloat(clearColor.R, clearColor.G, clearColor.B, clearColor.A));
            cl.ClearColorTarget(1, RgbaFloat.Black);
            cl.ClearDepthStencil(1f);
        }

        var mainLightDirection = Vector3.Normalize(scene.MainLight.Direction);

        CameraDataBuffer cameraInfo = new CameraDataBuffer();
        cameraInfo.ViewProjectionMatrix = camera.ViewProjectionMatrix;
        cameraInfo.ShadowMapMatrix0 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(0);
        cameraInfo.ShadowMapMatrix1 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(1);
        cameraInfo.ShadowMapMatrix2 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(2);
        cameraInfo.ShadowMapMatrix3 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(3);

        LightInfo lightInfo = new LightInfo();
        lightInfo.AmbientColor = scene.AmbientColor;
        lightInfo.MainLightColor = scene.MainLight.Color;
        lightInfo.ShadowColor = scene.MainLight.ShadowMap.Color;
        lightInfo.MainLightDirection = mainLightDirection;
        lightInfo.FogColor = scene.FogColor;
        lightInfo.FogStart = scene.FogStart; // / (camera.FarPlane - camera.NearPlane);
        lightInfo.FogEnd = scene.FogEnd; // / (camera.FarPlane - camera.NearPlane);
        lightInfo.ShadowMapDistances = this._shadowPass.GetShadowCascadeDistances();

        cl.UpdateBuffer(this._camera3DInfoBuffer, 0, ref cameraInfo);
        cl.UpdateBuffer(this._lightInfoBuffer, 0, ref lightInfo);

        this._renderJob.DrawRenderList(cl, this._resourceSet, this._opaqueRenderQueue);

        this._renderJob.DrawRenderList(cl, this._resourceSet, this._transparentRenderQueue);
    }

    public void Dispose()
    {
        this._resourceLayout.Dispose();
        this._resourceSet.Dispose();
        this._camera3DInfoBuffer.Dispose();
        this._lightInfoBuffer.Dispose();
    }

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant, RenderFlags flags)
    {
        var blendDescription = flags.HasFlag(RenderFlags.Transparent) ? BlendAttachmentDescription.AlphaBlend : BlendAttachmentDescription.OverrideBlend;
        var cullMode = flags.HasFlag(RenderFlags.DoubleSided) ? FaceCullMode.None : FaceCullMode.Back;
        var depthTestEnabled = flags.HasFlag(RenderFlags.DepthTest);
        var depthWriteEnabled = flags.HasFlag(RenderFlags.DepthWrite);
        var fillMode = flags.HasFlag(RenderFlags.Wireframe) ? PolygonFillMode.Wireframe : PolygonFillMode.Solid;
        var outputDescription = this._renderTexture.OutputDescription;
        var scissorTestEnabled = flags.HasFlag(RenderFlags.ScisorTest);
        if (!flags.HasFlag(RenderFlags.MousePick))
            outputDescription = new OutputDescription(outputDescription.DepthAttachment, outputDescription.ColorAttachments[0]);

        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled, depthWriteEnabled, ComparisonKind.LessEqual
            ),
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription(GetVertexLayout(shaderVariant.VertexFormat), shaderVariant.VeldridShaders),
            BlendState = new BlendStateDescription(
                RgbaFloat.Black,
                blendDescription,
                BlendAttachmentDescription.Disabled
            ),
            RasterizerState = new RasterizerStateDescription(
                cullMode,
                fillMode,
                FrontFace.CounterClockwise,
                depthClipEnabled: true,
                scissorTestEnabled
            ),
            Outputs = outputDescription,
            ResourceLayouts = this.GetResourceLayouts(shaderVariant),
        });
    }

    private static VertexLayoutDescription[] GetVertexLayout(VertexFormat vertexFormat)
    {
        var list = new List<VertexLayoutDescription>(vertexFormat.Layouts.Length + 1)
        {
            new VertexLayoutDescription(stride: 16, instanceStepRate: 1,
                new VertexElementDescription("Offsets", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt4))
        };
        list.AddRange(vertexFormat.Layouts);
        return list.ToArray();
    }

    private ResourceLayout[] GetResourceLayouts(ShaderVariant shaderVariant)
    {
        var resources = new List<ResourceLayout>
        {
            this._resourceLayout,
            this._renderer.TransformResourceLayout,
            shaderVariant.MaterialResourceLayout,
            this._renderer.InstanceResourceLayout
        };

        if (shaderVariant.VertexFormat.IsSkinned)
            resources.Add(this._renderer.SkeletonResourceLayout);

        return resources.ToArray();
    }
}
