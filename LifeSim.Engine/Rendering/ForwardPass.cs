using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class ForwardPass : IDisposable, IPipelineProvider, IRenderingPass
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
        public Vector4 ShadowMapDistances { get; set; } // Each component is the far plane of a cascade
    }

    private readonly Renderer _renderer;
    private readonly GraphicsDevice _gd;
    private readonly DeviceBuffer _lightInfoBuffer;
    private readonly DeviceBuffer _camera3DInfoBuffer;
    private readonly ResourceLayout _resourceLayout;
    private ResourceSet _resourceSet;
    private readonly IRenderTexture _renderTexture;
    private readonly SceneStorage _storage;
    private readonly RenderJob _renderJob;
    private readonly RenderQueue _opaqueRenderQueue;
    private readonly ImmediateBatcher _immediateModeBatcher;
    private readonly ShadowPass _shadowPass;

    private readonly List<ImmediateRenderNode3D> _immediateRenderNodes = new List<ImmediateRenderNode3D>();

    public ForwardPass(Renderer renderer, SceneStorage storage, IRenderTexture mainRenderTexture, ShadowPass shadowPass)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;
        this._storage = storage;
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

        this._shadowPass.ShadowmapTexture.OnResized += this.OnShadowmapResized;

        this._immediateModeBatcher = new ImmediateBatcher(this._gd, this._renderTexture);
    }

    private void OnShadowmapResized()
    {
        this._renderer.DisposeWhenIdle(this._resourceSet);
        this._resourceSet = this.CreatePassResourceSet();
    }

    public void AddImmediateRenderNode(ImmediateRenderNode3D node)
    {
        this._immediateRenderNodes.Add(node);
    }

    public void RemoveImmediateRenderNode(ImmediateRenderNode3D node)
    {
        this._immediateRenderNodes.Remove(node);
    }

    private ResourceSet CreatePassResourceSet()
    {
        return this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout,
            this._camera3DInfoBuffer,
            this._lightInfoBuffer,
            this._shadowPass.ShadowmapTexture.DeviceTexture,
            this._shadowPass.ShadowmapTexture.ShadowSampler
        ));
    }

    public void Render(CommandList cl, Scene scene)
    {
        Camera3D? camera = scene.Camera;

        if (camera == null)
        {
            return;
        }

        this._opaqueRenderQueue.AddToRenderQueue(camera.FrustumForCulling, camera.Position);
        this._opaqueRenderQueue.Sort();

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
        lightInfo.ShadowMapDistances = this._shadowPass.GetShadowCascadeDistances();

        cl.UpdateBuffer(this._camera3DInfoBuffer, 0, ref cameraInfo);
        cl.UpdateBuffer(this._lightInfoBuffer, 0, ref lightInfo);

        this._renderJob.DrawRenderList(cl, this._resourceSet, this._opaqueRenderQueue);


        if (this._immediateRenderNodes.Count > 0)
        {
            if (this._renderTexture is not RenderTexture renderTexture) return;
            cl.SetFramebuffer(renderTexture.ColorOnlyFramebuffer);
            this._immediateModeBatcher.Begin(cl, camera.ViewProjectionMatrix);
            for (int i = 0; i < this._immediateRenderNodes.Count; i++)
            {
                this._immediateRenderNodes[i].Render(this._immediateModeBatcher);
            }
            this._immediateModeBatcher.End();
        }
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
            ShaderSet = new ShaderSetDescription(GetVertexLayout(shaderVariant.VertexFormat), shaderVariant.Shaders),
            BlendState = new BlendStateDescription(
                RgbaFloat.Black,
                BlendAttachmentDescription.OverrideBlend,
                BlendAttachmentDescription.Disabled
            ),
            RasterizerState = rasterizerState,
            Outputs = this._renderTexture.OutputDescription,
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