using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Graphics.Materials;
using Imago.Graphics.Meshes;
using Imago.Graphics.Textures;
using Imago.SceneGraph;
using Imago.SceneGraph.Lighting;
using Support.Drawing;
using Veldrid;
using Veldrid.Utilities;
using Shader = Imago.Graphics.Materials.Shader;

namespace Imago.Graphics.Rendering;

internal class ForwardPass : IDisposable, IPipelineProvider
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
    private readonly RenderBatcher _renderBatcher;
    private readonly ShadowPass _shadowPass;

    public Shader DefaultShader { get; }

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

        this._renderBatcher = new RenderBatcher(this._gd, RenderBatchPassType.Forward);

        this._shadowPass.ShadowmapTexture.Resized += this.Shadowmap_Resized;

        RenderFlags supportedForwardFlags = RenderFlags.AlphaTest | RenderFlags.ReceiveShadows | RenderFlags.Fog | RenderFlags.PixelPerfactShadows | RenderFlags.ColorWrite | RenderFlags.ShadowCascades;
        var baseVertex = ShaderLoader.Load("base.vert.glsl");
        var baseFragment = ShaderLoader.Load("base.frag.glsl");
        this.DefaultShader = new Shader(renderer, this, baseVertex, baseFragment, new[] { "Surface" }, supportedForwardFlags);
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

    public void Render(CommandList cl, Stage stage, RenderTexture renderTexture)
    {
        var scene = stage.Scene;
        var camera = scene.Camera;
        if (camera == null) return;

        var env = scene.Environment;

        var frustumForCulling = new BoundingFrustum(camera.FrustumCullingCamera.ViewProjectionMatrix);
        stage.OpaqueRenderQueue.Update(frustumForCulling, camera.Position);
        stage.TransparentRenderQueue.Update(frustumForCulling, camera.Position);

        cl.SetFramebuffer(renderTexture.Framebuffer);

        CameraDataBuffer cameraInfo = new CameraDataBuffer();
        cameraInfo.ViewProjectionMatrix = camera.ViewProjectionMatrix;
        cameraInfo.ShadowMapMatrix0 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(0);
        cameraInfo.ShadowMapMatrix1 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(1);
        cameraInfo.ShadowMapMatrix2 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(2);
        cameraInfo.ShadowMapMatrix3 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(3);

        LightInfo lightInfo = new LightInfo();
        lightInfo.AmbientColor = env.AmbientColor;
        lightInfo.MainLightColor = env.MainLight.Color;
        lightInfo.ShadowColor = env.MainLight.ShadowMap.Color;
        lightInfo.MainLightDirection = env.MainLight.Direction;
        lightInfo.FogColor = env.FogColor;
        lightInfo.FogStart = env.FogStart; // / (camera.FarPlane - camera.NearPlane);
        lightInfo.FogEnd = env.FogEnd; // / (camera.FarPlane - camera.NearPlane);
        lightInfo.ShadowMapDistances = GetShadowCascadeDistances(env.MainLight.ShadowMap);

        cl.UpdateBuffer(this._camera3DInfoBuffer, 0, ref cameraInfo);
        cl.UpdateBuffer(this._lightInfoBuffer, 0, ref lightInfo);

        this._renderBatcher.DrawRenderList(cl, this._resourceSet, stage.OpaqueRenderQueue);

        this._renderBatcher.DrawRenderList(cl, this._resourceSet, stage.TransparentRenderQueue);
    }

    internal static Vector4 GetShadowCascadeDistances(ShadowMap shadowMap)
    {
        return new Vector4(shadowMap.SplitDistances[1], shadowMap.SplitDistances[2], shadowMap.SplitDistances[3], shadowMap.SplitDistances[4]);
    }

    public void Dispose()
    {
        this._resourceLayout.Dispose();
        this._resourceSet.Dispose();
        this._camera3DInfoBuffer.Dispose();
        this._lightInfoBuffer.Dispose();
    }

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount)
    {
        var blendDescription = flags.HasFlag(RenderFlags.Transparent) ? BlendAttachmentDescription.AlphaBlend : BlendAttachmentDescription.OverrideBlend;
        var cullMode = flags.HasFlag(RenderFlags.DoubleSided) ? FaceCullMode.None : FaceCullMode.Back;
        var depthTestEnabled = flags.HasFlag(RenderFlags.DepthTest);
        var depthWriteEnabled = flags.HasFlag(RenderFlags.DepthWrite);
        var fillMode = flags.HasFlag(RenderFlags.Wireframe) ? PolygonFillMode.Wireframe : PolygonFillMode.Solid;
        var scissorTestEnabled = flags.HasFlag(RenderFlags.ScisorTest);

        var outputDescription = new OutputDescription( // WARNING: This should be changed in RenderTexture if changes
            new OutputAttachmentDescription(PixelFormat.D32_Float_S8_UInt),
            new [] { new OutputAttachmentDescription(PixelFormat.R8_G8_B8_A8_UNorm) },
            sampleCount
        );

        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = new DepthStencilStateDescription(
                depthTestEnabled, depthWriteEnabled, ComparisonKind.LessEqual
            ),
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription(GetVertexLayout(shaderVariant.VertexFormat), shaderVariant.VeldridShaders),
            BlendState = new BlendStateDescription(RgbaFloat.Black, blendDescription),
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
