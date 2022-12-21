using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.SceneGraph;
using LifeSim.Utils;
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

    private readonly ShadowCascade[] _cascades = new ShadowCascade[4];
    private Matrix4x4 _scalingMatrix;

    private readonly float[] _splitDistances = new float[5]; // 4 splits + 1 for far plane

    private readonly RenderQueue[] _renderQueues;

    public ShadowPass(Renderer renderer, SceneStorage storage)
    {
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;
        this._storage = storage;

        this._renderQueues = new RenderQueue[4];
        for (int i = 0; i < this._renderQueues.Length; i++)
        {
            this._renderQueues[i] = new RenderQueue(RenderQueues.ShadowCaster);
        }

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ShadowMapDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this.ShadowmapTexture = new ShadowMapTexture(renderer, size: 16, cascadesCount: 1);

        this._shadowmapInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<ShadowMapDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._shadowmapInfoBuffer));

        this._renderJob = new RenderJob(this._gd, true);

        float verticalFlip = this._gd.IsUvOriginTopLeft ? -1.0f : 1.0f;
        this._scalingMatrix = Matrix4x4.CreateScale(.5f, .5f * verticalFlip, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f);

        for (int i = 0; i < this._cascades.Length; i++)
        {
            this._cascades[i] = new ShadowCascade();
        }
    }

    public void Render(CommandList commandList, Scene scene)
    {
        Camera3D? camera = scene.Camera;
        if (camera == null)
        {
            return;
        }

        ShadowMap shadowMap = scene.MainLight.ShadowMap;

        this.UpdateSplitDistances(camera, shadowMap);

        this.UpdateShadowMap(shadowMap);

        var mainLightDirection = scene.MainLight.Direction;

        for (int i = 0; i < shadowMap.CascadesCount; i++)
        {
            commandList.SetFramebuffer(this.ShadowmapTexture.Framebuffers[i]);
            commandList.ClearDepthStencil(1f);

            float near = this._splitDistances[i];
            float far = this._splitDistances[i + 1];

            this._cascades[i].UpdateCascadeMatrix(i, camera, mainLightDirection, near, far, shadowMap);

            BoundingFrustum shadowFrustum = new BoundingFrustum(this._cascades[i].ViewProjectionMatrix);
            this._renderQueues[i].Update(shadowFrustum, camera.Position);

            ShadowMapDataBuffer data = new ShadowMapDataBuffer();
            data.ShadowMapMatrix = this._cascades[i].ViewProjectionMatrix;
            data.ShadowBias = new Vector2(this._cascades[i].DepthBias, this._cascades[i].NormalOffset);
            data.LightDirection = mainLightDirection;

            commandList.UpdateBuffer(this._shadowmapInfoBuffer, 0, data);
            this._renderJob.DrawRenderList(commandList, this._resourceSet, this._renderQueues[i]);
        }
    }

    public Matrix4x4 GetShadowCascadeViewProjectionMatrix(int cascadeIndex)
    {
        if (cascadeIndex < 0 || cascadeIndex >= this.ShadowmapTexture.CascadesCount)
        {
            return Matrix4x4.Identity;
        }

        return this._cascades[cascadeIndex].ViewProjectionMatrix * this._scalingMatrix;
    }

    private void UpdateShadowMap(ShadowMap shadowMap)
    {
        var texture = this.ShadowmapTexture;
        if (shadowMap.Size != texture.Size || shadowMap.CascadesCount != texture.CascadesCount)
        {
            this.ShadowmapTexture.Resize(shadowMap.Size, shadowMap.CascadesCount);
        }
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

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant, RenderFlags flags)
    {
        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription(GetVertexLayout(shaderVariant.VertexFormat), shaderVariant.VeldridShaders),
            BlendState = BlendStateDescription.Empty,
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.Back,
                PolygonFillMode.Solid,
                FrontFace.CounterClockwise,
                depthClipEnabled: false, // Shadow pancaking! Love pancakes!
                scissorTestEnabled: false
            ),
            Outputs = this.ShadowmapTexture.Framebuffers[0].OutputDescription,
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

    internal Vector4 GetShadowCascadeDistances()
    {
        return new Vector4(this._splitDistances[1], this._splitDistances[2], this._splitDistances[3], this._splitDistances[4]);
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

    public void UpdateSplitDistances(Camera3D camera, ShadowMap shadowMap)
    {
        // Lerp between uniform and logarithmic split distances.
        // https://developer.nvidia.com/gpugems/gpugems3/part-ii-light-and-shadows/chapter-10-parallel-split-shadow-maps-programmable-gpus

        float near = camera.NearPlane;
        float far = MathF.Min(camera.FarPlane, camera.NearPlane + shadowMap.MaximumShadowsDistance);
        far = MathF.Max(far, near + 0.01f);

        uint count = shadowMap.CascadesCount;

        this._splitDistances[0] = near;
        this._splitDistances[count] = far;

        for (int i = 1; i < count; i++)
        {
            float t = (float)i / count;
            float uniformDistance = near + (far - near) * t;
            float logarithmicDistance = near * MathF.Pow(far / near, t);
            this._splitDistances[i] = MathUtils.Lerp(logarithmicDistance, uniformDistance, shadowMap.SplitLambda);
        }
    }
}