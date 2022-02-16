using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class SpritesPass : IDisposable, IPipelineProvider, IRenderingPass
{
    private readonly DeviceBuffer _camera2DInfoBuffer;
    private readonly ResourceSet _passResourceSet;
    private readonly IRenderTexture _renderTexture;
    private readonly ResourceLayout _passResourceLayout;
    private readonly GraphicsDevice _gd;

    private Shader? _currentShader;
    private readonly VertexFormat _vertexFormat;
    private readonly Dictionary<(Shader, Texture), ResourceSet> _resourceSets = new Dictionary<(Shader, Texture), ResourceSet>();
    private readonly Shader _defaultShader;

    private readonly ResourceLayout _resourceLayout;

    private readonly SpriteBatcher _spriteBatcher;

    public SpritesPass(Renderer renderer, IRenderTexture renderTexture)
    {
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._passResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._vertexFormat = new VertexFormat(new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
        ));

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        var vertex = ShaderLoader.Load("sprites.vert.glsl");
        var fragment = ShaderLoader.Load("sprites.frag.glsl");
        this._defaultShader = new Shader(this, vertex, fragment, this._resourceLayout);

        this._camera2DInfoBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._passResourceLayout, this._camera2DInfoBuffer));

        this._renderTexture = renderTexture;

        this._spriteBatcher = new SpriteBatcher(this._gd, this._defaultShader);
    }

    public Shader MakeShader(string vertexFile, string fragmentFile)
    {
        var vertex = ShaderLoader.Load(vertexFile);
        var fragment = ShaderLoader.Load(fragmentFile);
        return new Shader(this, vertex, fragment, this._resourceLayout);
    }

    public void Dispose()
    {
        this._camera2DInfoBuffer.Dispose();
        this._passResourceLayout.Dispose();
        this._passResourceSet.Dispose();

        foreach (var set in this._resourceSets.Values)
        {
            set.Dispose();
        }
    }

    public void BeginPass(CommandList commandList, Matrix4x4 projectionMatrix)
    {
        commandList.SetFramebuffer(this._renderTexture.Framebuffer);
        commandList.ClearDepthStencil(1f);
        commandList.UpdateBuffer(this._camera2DInfoBuffer, 0, ref projectionMatrix);

        this._currentShader = null;
    }

    public void SubmitBatches(CommandList commandList, DeviceBuffer sharedIndexBuffer, IReadOnlyList<SpriteBatch> batches)
    {
        for (int i = 0; i < batches.Count; i++)
        {
            var batch = batches[i];

            commandList.UpdateBuffer(batch.VertexBuffer, 0, batch.Items);

            if (this._currentShader != batch.Shader)
            {
                this._currentShader = batch.Shader;
                var pipeline = batch.Shader.GetPipeline(this._vertexFormat);

                commandList.SetPipeline(pipeline);
                commandList.SetGraphicsResourceSet(0, this._passResourceSet);
            }

            commandList.SetVertexBuffer(0, batch.VertexBuffer);
            commandList.SetIndexBuffer(sharedIndexBuffer, IndexFormat.UInt16);

            commandList.SetGraphicsResourceSet(1, batch.ResourceSet);
            commandList.DrawIndexed(
                indexCount: (uint)batch.Count * 6,
                instanceCount: 1,
                indexStart: 0,
                vertexOffset: 0,
                instanceStart: 0
            );
        }
    }

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant)
    {
        Debug.Assert(shaderVariant.MaterialResourceLayout != null);

        var rasterizerState = new RasterizerStateDescription(
            FaceCullMode.None,
            PolygonFillMode.Solid,
            FrontFace.Clockwise,
            depthClipEnabled: true,
            scissorTestEnabled: false
        );

        var resources = new ResourceLayout[] {
            this._passResourceLayout,
            shaderVariant.MaterialResourceLayout,
        };

        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = shaderVariant.ShaderSetDescription,
            BlendState = BlendStateDescription.SingleAlphaBlend,
            RasterizerState = rasterizerState,
            Outputs = this._renderTexture.OutputDescription,
            ResourceLayouts = resources,
        });
    }

    public void Render(CommandList cl, Scene scene)
    {
        var canvasLayers = scene.CanvasLayers;

        for (int i = 0; i < canvasLayers.Count; i++)
        {
            var canvasLayer = canvasLayers[i];
            this._spriteBatcher.BeginBatch();

            for (int j = 0; j < canvasLayer.Items.Count; j++)
            {
                canvasLayer.Items[j].Render(this._spriteBatcher);
            }

            this.BeginPass(cl, canvasLayer.ViewProjectionMatrix);
            this.SubmitBatches(cl, this._spriteBatcher.IndexBuffer, this._spriteBatcher.Batches);
        }

        if (scene.UILayer != null)
        {
            this._spriteBatcher.BeginBatch();
            scene.UILayer.Draw(this._spriteBatcher);
            this.BeginPass(cl, scene.UILayer.ViewProjectionMatrix);
            this.SubmitBatches(cl, this._spriteBatcher.IndexBuffer, this._spriteBatcher.Batches);
        }
    }
}