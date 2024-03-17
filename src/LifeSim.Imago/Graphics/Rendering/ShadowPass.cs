using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Meshes;
using LifeSim.Imago.SceneGraph;
using LifeSim.Imago.SceneGraph.Lighting;
using Veldrid;
using Veldrid.Utilities;
using Shader = LifeSim.Imago.Graphics.Materials.Shader;

namespace LifeSim.Imago.Graphics.Rendering;

internal class ShadowPass : IDisposable, IPipelineProvider
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
    private readonly Renderer _renderer;
    private readonly DeviceBuffer _shadowmapInfoBuffer;
    private readonly RenderBatcher _renderBatcher;

    private readonly ShadowCascade[] _cascades = new ShadowCascade[4];
    private Matrix4x4 _scalingMatrix;
    public Shader DefaultShader { get; }

    public ShadowPass(Renderer renderer)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("ShadowMapDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this.ShadowmapTexture = new ShadowMapTexture(renderer, size: 16, cascadesCount: 1);

        this._shadowmapInfoBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<ShadowMapDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._resourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._resourceLayout, this._shadowmapInfoBuffer));

        this._renderBatcher = new RenderBatcher(this._gd, RenderBatchPassType.ShadowMap);

        float verticalFlip = this._gd.IsUvOriginTopLeft ? -1.0f : 1.0f;
        this._scalingMatrix = Matrix4x4.CreateScale(.5f, .5f * verticalFlip, 1f) * Matrix4x4.CreateTranslation(0.5f, 0.5f, 0f);

        for (int i = 0; i < this._cascades.Length; i++)
        {
            this._cascades[i] = new ShadowCascade();
        }


        RenderFlags supportedShadowMapFlags = RenderFlags.AlphaTest;
        var shadowmapVertex = ShaderLoader.Load("shadowmap.vert.glsl");
        var shadowmapFragment = ShaderLoader.Load("shadowmap.frag.glsl");
        this.DefaultShader = new Shader(renderer, this, shadowmapVertex, shadowmapFragment, new[] { "Surface" }, supportedShadowMapFlags);
    }

    public void Render(CommandList cl, Stage stage)
    {
        var scene = stage.Scene;
        var camera = scene.Camera;
        if (camera == null) return;

        var mainLight = scene.Environment.MainLight;
        var shadowMap = mainLight.ShadowMap;

        int cascadesCount = stage.CascadesCount;

        this.UpdateShadowMap(shadowMap, cascadesCount);

        for (int i = 0; i < cascadesCount; i++)
        {
            cl.SetFramebuffer(this.ShadowmapTexture.Framebuffers[i]);
            cl.ClearDepthStencil(1f);

            float near = shadowMap.SplitDistances[i];
            float far = shadowMap.SplitDistances[i + 1];

            this._cascades[i].UpdateCascadeMatrix(i, camera, mainLight.Direction, near, far, shadowMap);

            BoundingFrustum shadowFrustum = new BoundingFrustum(this._cascades[i].ViewProjectionMatrix);

            var renderQueue = stage.ShadowCasterRenderQueues[i];
            renderQueue.Update(shadowFrustum, camera.Position);

            ShadowMapDataBuffer data = new ShadowMapDataBuffer();
            data.ShadowMapMatrix = this._cascades[i].ViewProjectionMatrix;
            data.ShadowBias = new Vector2(this._cascades[i].DepthBias, this._cascades[i].NormalOffset);
            data.LightDirection = mainLight.Direction;

            cl.UpdateBuffer(this._shadowmapInfoBuffer, 0, data);
            this._renderBatcher.DrawRenderList(cl, this._resourceSet, renderQueue);
        }
    }

    public Matrix4x4 GetShadowCascadeViewProjectionMatrix(int cascadeIndex)
    {
        if (cascadeIndex < 0 || cascadeIndex >= this.ShadowmapTexture.CascadesCount)
            return Matrix4x4.Identity;

        return this._cascades[cascadeIndex].ViewProjectionMatrix * this._scalingMatrix;
    }

    private void UpdateShadowMap(ShadowMap shadowMap, int cascadesCount)
    {
        var texture = this.ShadowmapTexture;
        if (shadowMap.Size != texture.Size || cascadesCount != texture.CascadesCount)
            this.ShadowmapTexture.Resize(shadowMap.Size, (uint)cascadesCount);
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

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount)
    {
        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription(GetVertexLayout(shaderVariant.VertexFormat), shaderVariant.VeldridShaders),
            BlendState = BlendStateDescription.Empty,
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.None, // Shadows are two-sided
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
