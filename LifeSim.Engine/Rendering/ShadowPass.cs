using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LifeSim.Engine.SceneGraph;
using Veldrid;
using Veldrid.Utilities;

namespace LifeSim.Engine.Rendering;

public partial class ShadowPass : IDisposable, IPipelineProvider, IRenderingPass
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ShadowMapDataBuffer
    {
        public Matrix4x4 ShadowMapMatrix { get; set; }
        public Vector2 ShadowBias { get; set; } // x = depth bias, y = normal bias
        private readonly Vector2 _padding0;
        public Vector3 LightDirection { get; set; } // xyz = light direction
        private readonly float _padding1;
    }

    public ShadowMapTexture ShadowmapTexture { get; private set; }

    private readonly ResourceLayout _resourceLayout;
    private readonly ResourceSet _resourceSet;
    private readonly GraphicsDevice _gd;
    private readonly DeviceBuffer _shadowmapInfoBuffer;
    private readonly RenderJob _renderJob;
    private readonly SceneStorage _storage;

    private ShadowCascade[] _cascades { get; } = new ShadowCascade[4];
    private Matrix4x4 _scalingMatrix;

    public ShadowMapConfig Config { get; }

    private bool _shadowMapTextureSizeDirty = false;

    private readonly float[] _splitDistances = new float[5]; // 4 splits + 1 for far plane

    private readonly RenderQueue[] _renderQueues;

    public ShadowPass(Renderer renderer, SceneStorage storage)
    {
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;
        this.Config = new ShadowMapConfig();
        this._storage = storage;

        this._renderQueues = new RenderQueue[4];
        for (int i = 0; i < this._renderQueues.Length; i++)
        {
            this._renderQueues[i] = new RenderQueue();
        }

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ShadowMapDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        uint size = this.Config.ShadowMapResolution;
        uint count = this.Config.CascadesCount;
        this.ShadowmapTexture = new ShadowMapTexture(this._gd, size, size, count);

        this._shadowmapInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<ShadowMapDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._shadowmapInfoBuffer));

        this._renderJob = new RenderJob(this._gd, this._resourceSet, true);

        this.Config.OnCascadeCountChanged += this.OnCascadeCountChanged;
        this.Config.OnShadowMapSizeChanged += this.OnShadowMapSizeChanged;

        float verticalFlip = this._gd.IsUvOriginTopLeft ? -1.0f : 1.0f;
        this._scalingMatrix = Matrix4x4.CreateScale(.5f, .5f * verticalFlip, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f);

        for (int i = 0; i < this._cascades.Length; i++)
        {
            this._cascades[i] = new ShadowCascade();
        }
    }

    private void OnShadowMapSizeChanged(uint size)
    {
        this._shadowMapTextureSizeDirty = true;
    }

    private void OnCascadeCountChanged(uint cascadesCount)
    {
        this._shadowMapTextureSizeDirty = true;
    }

    public void Render(CommandList commandList, Scene scene)
    {
        Camera3D? camera = scene.Camera;
        if (camera == null)
        {
            return;
        }

        this.UpdateSplitDistances(camera);

        this.UpdateShadowMapTextureSize();

        var renderables = scene.Renderables;
        var mainLightDirection = scene.MainLight.Direction;

        for (int i = 0; i < this.Config.CascadesCount; i++)
        {
            commandList.SetFramebuffer(this.ShadowmapTexture.Framebuffers[i]);
            commandList.ClearDepthStencil(1f);

            float near = this._splitDistances[i];
            float far = this._splitDistances[i + 1];

            this._cascades[i].UpdateCascadeMatrix(i, camera, mainLightDirection, near, far, this.Config);

            BoundingFrustum shadowFrustum = new BoundingFrustum(this._cascades[i].ViewProjectionMatrix);
            this._renderQueues[i].AddToRenderQueue(renderables, shadowFrustum, camera.Position);

            ShadowMapDataBuffer data = new ShadowMapDataBuffer();
            data.ShadowMapMatrix = this._cascades[i].ViewProjectionMatrix;
            data.ShadowBias = new Vector2(this._cascades[i].DepthBias, this._cascades[i].NormalOffset);
            data.LightDirection = mainLightDirection;

            commandList.UpdateBuffer(this._shadowmapInfoBuffer, 0, data);
            this._renderJob.DrawRenderList(commandList, this._renderQueues[i]);
        }
    }

    public Matrix4x4 GetShadowCascadeViewProjectionMatrix(int cascadeIndex)
    {
        if (cascadeIndex < 0 || cascadeIndex >= this.Config.CascadesCount)
        {
            return Matrix4x4.Identity;
        }

        return this._cascades[cascadeIndex].ViewProjectionMatrix * this._scalingMatrix;
    }

    private void UpdateShadowMapTextureSize()
    {
        if (!this._shadowMapTextureSizeDirty) return;

        uint count = this.Config.CascadesCount;
        uint size = this.Config.ShadowMapResolution;
        this._gd.DisposeWhenIdle(this.ShadowmapTexture);
        this.ShadowmapTexture = new ShadowMapTexture(this._gd, size, size, count);
        this._shadowMapTextureSizeDirty = false;
    }

    public void Dispose()
    {
        this._resourceSet.Dispose();
        this._resourceLayout.Dispose();
        this.ShadowmapTexture.Dispose();
        this._shadowmapInfoBuffer.Dispose();
    }

    internal Vector4 GetShadowBiasData(int index)
    {
        var cascade = this._cascades[index];
        return new Vector4(cascade.DepthBias, cascade.NormalOffset, 0.0f, 0.0f);
    }

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant)
    {
        var rasterizerState = new RasterizerStateDescription(
            FaceCullMode.Front,
            PolygonFillMode.Solid,
            FrontFace.Clockwise,
            depthClipEnabled: false, // Shadow pancaking! Love pancakes!
            scissorTestEnabled: false
        );

        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = shaderVariant.ShaderSetDescription,
            BlendState = BlendStateDescription.Empty,
            RasterizerState = rasterizerState,
            Outputs = this.ShadowmapTexture.Framebuffers[0].OutputDescription,
            ResourceLayouts = this.GetResourceLayouts(shaderVariant),
        });
    }

    internal Vector4 GetShadowCascadeDistances()
    {
        return new Vector4(this._splitDistances[1], this._splitDistances[2], this._splitDistances[3], this._splitDistances[4]);
    }

    private ResourceLayout[] GetResourceLayouts(ShaderVariant shaderVariant)
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

    public void UpdateSplitDistances(Camera3D camera)
    {
        // Lerp between uniform and logarithmic split distances.
        // https://developer.nvidia.com/gpugems/gpugems3/part-ii-light-and-shadows/chapter-10-parallel-split-shadow-maps-programmable-gpus

        float near = camera.NearPlane;
        float far = MathF.Min(camera.FarPlane, camera.NearPlane + this.Config.MaximumShadowsDistance);
        far = MathF.Max(far, near + 0.01f);

        uint count = this.Config.CascadesCount;

        this._splitDistances[0] = near;
        this._splitDistances[count] = far;

        for (int i = 1; i < count; i++)
        {
            float t = (float)i / count;
            float uniformDistance = near + (far - near) * t;
            float logarithmicDistance = near * MathF.Pow(far / near, t);
            this._splitDistances[i] = MathUtils.Lerp(logarithmicDistance, uniformDistance, this.Config.SplitLambda);
        }
    }
}