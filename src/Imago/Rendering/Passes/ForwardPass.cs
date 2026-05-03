using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Assets.Materials;
using Imago.Assets.Meshes;
using Imago.Assets.Textures;
using Imago.Rendering.Internals;
using Imago.Rendering.Passes.Shadows;
using Imago.SceneGraph;
using Imago.SceneGraph.Cameras;
using Imago.SceneGraph.Lighting;
using Imago.Support.Drawing;
using Imago.Utilities;
using NeoVeldrid;

namespace Imago.Rendering.Passes;

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
    private struct GlobalData
    {
        public Vector3 CameraPosition { get; set; }
        public float Time { get; set; }
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
    private readonly DeviceBuffer _globalDataBuffer;
    private readonly ResourceLayout _resourceLayout;
    private ResourceSet _resourceSet;
    private readonly RenderBatcher _renderBatcher;
    private readonly ShadowPass _shadowPass;

    public ForwardPass(Renderer renderer, ShadowPass shadowPass)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;
        this._shadowPass = shadowPass;

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("LightDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("ShadowMapTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("ShadowMapSampler", ResourceKind.Sampler, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("GlobalDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment)
        ));

        this._camera3DInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<CameraDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        this._lightInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<LightInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        this._globalDataBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<GlobalData>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._resourceSet = this.CreatePassResourceSet();

        this._renderBatcher = new RenderBatcher(this._gd, RenderBatchPassType.Forward, renderer.Statistics);

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
            this._shadowPass.ShadowmapTexture.NativeTexture,
            this._shadowPass.ShadowmapTexture.ShadowSampler,
            this._globalDataBuffer
        ));
    }

    /// <summary>
    /// Prepares the forward pass for the current frame. Culls the given queues and uploads camera, lighting, and global uniforms.
    /// </summary>
    /// <param name="cl">The command list to use for rendering.</param>
    /// <param name="renderTexture">The render texture to render the scene to.</param>
    /// <param name="camera">The camera to use for rendering.</param>
    /// <param name="environment">The scene environment to use for rendering.</param>
    /// <param name="opaque">The render queue for opaque objects.</param>
    /// <param name="transparent">The render queue for transparent objects.</param>
    public void Prepare(CommandList cl, RenderTexture renderTexture, Camera camera, SceneEnvironment environment, RenderQueue opaque, RenderQueue transparent)
    {
        var frustumForCulling = new BoundingFrustum(camera.FrustumCullingCamera.ViewProjectionMatrix);
        opaque.Update(frustumForCulling, camera.Position);
        transparent.Update(frustumForCulling, camera.Position);

        var stats = this._renderer.Statistics;
        stats.CurrentVisibleRenderables = opaque.Count + transparent.Count;
        stats.CurrentTotalRenderables = opaque.TotalCount + transparent.TotalCount;

        cl.SetFramebuffer(renderTexture.Framebuffer);

        CameraDataBuffer cameraInfo = new CameraDataBuffer();
        cameraInfo.ViewProjectionMatrix = camera.ViewProjectionMatrix;
        cameraInfo.ShadowMapMatrix0 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(0);
        cameraInfo.ShadowMapMatrix1 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(1);
        cameraInfo.ShadowMapMatrix2 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(2);
        cameraInfo.ShadowMapMatrix3 = this._shadowPass.GetShadowCascadeViewProjectionMatrix(3);

        LightInfo lightInfo = new LightInfo();
        lightInfo.AmbientColor = environment.AmbientColor;
        lightInfo.MainLightColor = environment.MainLight.Color;
        lightInfo.ShadowColor = environment.MainLight.ShadowMap.Color;
        lightInfo.MainLightDirection = environment.MainLight.Direction;
        lightInfo.FogColor = environment.FogColor;
        lightInfo.FogStart = environment.FogStart;
        lightInfo.FogEnd = environment.FogEnd;
        lightInfo.ShadowMapDistances = GetShadowCascadeDistances(environment.MainLight.ShadowMap);

        GlobalData globalData = new GlobalData();
        globalData.CameraPosition = camera.Position;
        globalData.Time = this._renderer.TotalTime;

        cl.UpdateBuffer(this._camera3DInfoBuffer, 0, ref cameraInfo);
        cl.UpdateBuffer(this._lightInfoBuffer, 0, ref lightInfo);
        cl.UpdateBuffer(this._globalDataBuffer, 0, ref globalData);
    }

    /// <summary>
    /// Draws a render queue. Must be called after <see cref="Prepare"/> in the same frame.
    /// </summary>
    /// <param name="cl">The command list to use for rendering.</param>
    /// <param name="queue">The render queue to draw.</param>
    public void Draw(CommandList cl, RenderQueue queue)
    {
        this._renderBatcher.DrawRenderList(cl, this._resourceSet, queue);
    }

    private static Vector4 GetShadowCascadeDistances(ShadowMap shadowMap)
    {
        return new Vector4(shadowMap.SplitDistances[1], shadowMap.SplitDistances[2], shadowMap.SplitDistances[3], shadowMap.SplitDistances[4]);
    }

    public void Dispose()
    {
        this._resourceLayout.Dispose();
        this._resourceSet.Dispose();
        this._camera3DInfoBuffer.Dispose();
        this._lightInfoBuffer.Dispose();
        this._globalDataBuffer.Dispose();
        this._renderBatcher.Dispose();
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
            ShaderSet = new ShaderSetDescription(GetVertexLayout(shaderVariant.VertexFormat), shaderVariant.NativeShaders),
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
