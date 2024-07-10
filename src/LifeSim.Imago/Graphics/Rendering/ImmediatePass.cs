using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Meshes;
using LifeSim.Imago.Graphics.Textures;
using LifeSim.Imago.SceneGraph;
using LifeSim.Support.Drawing;
using Veldrid;
using Shader = LifeSim.Imago.Graphics.Materials.Shader;
using Texture = LifeSim.Imago.Graphics.Textures.Texture;

namespace LifeSim.Imago.Graphics.Rendering;

/// <summary>
/// This class is used to batch draw calls together in immediate mode.
/// </summary>
public class ImmediatePass : IPipelineProvider, IDisposable, IImediateRenderer
{
    private struct PassDataBuffer
    {
        public Matrix4x4 ViewProjection;
    }

    private struct Vertex
    {
        public Vector3 Position { get; set; }

        public Vector2 TextureCoords { get; set; }

        public uint Color { get; set; }

        public Vertex(Vector3 position, Vector2 textureCoords, Color color)
        {
            this.Position = position;
            this.TextureCoords = textureCoords;
            this.Color = color.ToPackedUInt();
        }
    }

    private readonly GraphicsDevice _gd;

    private readonly DeviceBuffer _indexBuffer;

    private readonly DeviceBuffer _vertexBuffer;

    private readonly int _maxVertexBatchSize = 1024; // Number of vertices to batch before drawing. (otherwise, flush)

    private readonly int _maxIndexBatchSize = 1024; // Number of indices to batch before drawing. (otherwise, flush)

    private readonly Vertex[] _vertices;

    private readonly ushort[] _indices;

    private readonly ResourceLayout _passResourceLayout;

    private readonly DeviceBuffer _passDataBuffer;

    private readonly ResourceSet _passResourceSet;

    private readonly Shader _defaultShader;

    private readonly VertexFormat _vertexFormat;

    private readonly ResourceSetCache _resourceSetCache;

    private readonly ITexture _defaultTexture;

    private readonly Renderer _renderer;

    private Shader _currentBatchShader = null!;

    private Shader _currentShaderInUse = null!;

    private ITexture _currentBatchTexture = null!;

    private int _vertexCount = 0;

    private int _indexCount = 0;

    private CommandList _commandList = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImmediatePass"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    public ImmediatePass(Renderer renderer)
    {
        this._renderer = renderer;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._vertexBuffer = factory.CreateBuffer(new BufferDescription(
            (uint)(this._maxVertexBatchSize * Unsafe.SizeOf<BasicVertex>()),
            BufferUsage.VertexBuffer | BufferUsage.Dynamic));

        this._indexBuffer = factory.CreateBuffer(new BufferDescription(
            (uint)(this._maxIndexBatchSize * sizeof(ushort)),
            BufferUsage.IndexBuffer | BufferUsage.Dynamic));

        this._passDataBuffer = factory.CreateBuffer(new BufferDescription(
            (uint)Unsafe.SizeOf<PassDataBuffer>(),
            BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._vertices = new Vertex[this._maxVertexBatchSize];
        this._indices = new ushort[this._maxIndexBatchSize];

        this._passResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("PassDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(
            this._passResourceLayout,
            this._passDataBuffer
        ));

        this._vertexFormat = new VertexFormat("ImmediateVertex", new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)));

        this._defaultShader = new Shader(renderer, this, _vertexShader, _fragmentShader, new[] { "Main" });

        this._resourceSetCache = new ResourceSetCache(factory);

        this._defaultTexture = Texture.White;
    }

    public void Render(CommandList cl, Stage stage, RenderTexture renderTexture)
    {
        var scene = stage.Scene;
        var camera = scene.Camera;
        if (camera == null) return;

        var renderables = stage.ImmediateRenderables;
        if (renderables.Count == 0) return;

        cl.SetFramebuffer(renderTexture.Framebuffer);

        this.Begin(cl, camera.ViewProjectionMatrix);

        for (var i = 0; i < renderables.Count; i++)
        {
            renderables[i].Render(this);
        }

        this.End();
    }

    /// <summary>
    /// Begins a new batch.
    /// </summary>
    /// <param name="cl">The command list to record the batch to.</param>
    /// <param name="viewProjectionMatrix">The view projection matrix.</param>
    public void Begin(CommandList cl, Matrix4x4 viewProjectionMatrix)
    {
        this._commandList = cl;
        this._vertexCount = 0;
        this._indexCount = 0;
        this._currentBatchShader = null!;
        this._currentBatchTexture = null!;
        this._currentShaderInUse = null!;

        cl.UpdateBuffer(this._passDataBuffer, 0, new PassDataBuffer { ViewProjection = viewProjectionMatrix });
    }

    private void Prepare(int indexCount, int vertexCount)
    {
        if (this._vertexCount + vertexCount > this._maxVertexBatchSize)
            this.Flush();

        if (this._indexCount + indexCount > this._maxIndexBatchSize)
            this.Flush();

        // validate buffers overflow
        if (this._vertexCount + vertexCount > this._maxVertexBatchSize)
            throw new InvalidOperationException($"The number of provided vertices exceeds the maximum batch size. {this._vertexCount + vertexCount} > {this._maxVertexBatchSize}");

        if (this._indexCount + indexCount > this._maxIndexBatchSize)
            throw new InvalidOperationException($"The number of provided indices exceeds the maximum batch size. {this._indexCount + indexCount} > {this._maxIndexBatchSize}");
    }

    /// <summary>
    /// Sets the shader to use for the next subsequent draw calls.
    /// </summary>
    /// <param name="shader">The shader to use or null to use the default shader.</param>
    public void SetShader(Shader? shader)
    {
        shader ??= this._defaultShader;
        if (this._currentBatchShader != shader)
        {
            this.Flush();
            this._currentBatchShader = shader;
        }
    }

    /// <summary>
    /// Sets the texture to use for the next subsequent draw calls.
    /// </summary>
    /// <param name="texture">The texture to use or null to use the default texture.</param>
    public void SetTexture(ITexture? texture)
    {
        texture ??= this._defaultTexture;
        if (this._currentBatchTexture != texture)
        {
            this.Flush();
            this._currentBatchTexture = texture;
        }
    }

    /// <summary>
    /// Draws a batch of vertices in immediate mode.
    /// </summary>
    /// <param name="indices">The indices of the vertices to draw.</param>
    /// <param name="positions">The positions of the vertices to draw.</param>
    /// <param name="texCoords">The texture coordinates of the vertices to draw.</param>
    /// <param name="color">The color to tint the vertices with.</param>
    public void DrawVertices(ushort[] indices, Vector3[] positions, Vector2[] texCoords, Color color)
    {
        Debug.Assert(positions.Length == texCoords.Length, "The number of positions and texture coordinates must be the same.");

        this.Prepare(indices.Length, positions.Length);

        Array.Copy(indices, 0, this._indices, this._indexCount, indices.Length);

        for (int i = 0; i < positions.Length; i++)
        {
            this._vertices[this._vertexCount + i] = new Vertex(positions[i], texCoords[i], color);
        }

        this._vertexCount += positions.Length;
        this._indexCount += indices.Length;
    }

    /// <summary>
    /// Draws a quad in immediate mode. The quad is drawn using two triangles. The vertices should be in counter-clockwise order.
    /// </summary>
    /// <param name="v1">The first vertex.</param>
    /// <param name="v2">The second vertex.</param>
    /// <param name="v3">The third vertex.</param>
    /// <param name="v4">The fourth vertex.</param>
    /// <param name="t1">The first texture coordinate.</param>
    /// <param name="t2">The second texture coordinate.</param>
    /// <param name="t3">The third texture coordinate.</param>
    /// <param name="t4">The fourth texture coordinate.</param>
    /// <param name="color">The color to tint the quad with.</param>
    public void DrawQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector2 t1, Vector2 t2, Vector2 t3, Vector2 t4, Color color)
    {
        this.Prepare(6, 4);

        this._vertices[this._vertexCount + 0] = new Vertex(v1, t1, color);
        this._vertices[this._vertexCount + 1] = new Vertex(v2, t2, color);
        this._vertices[this._vertexCount + 2] = new Vertex(v3, t3, color);
        this._vertices[this._vertexCount + 3] = new Vertex(v4, t4, color);

        int i = this._vertexCount;
        this._indices[this._indexCount + 0] = (ushort)i;
        this._indices[this._indexCount + 1] = (ushort)(i + 1);
        this._indices[this._indexCount + 2] = (ushort)(i + 2);

        this._indices[this._indexCount + 3] = (ushort)i;
        this._indices[this._indexCount + 4] = (ushort)(i + 2);
        this._indices[this._indexCount + 5] = (ushort)(i + 3);

        this._vertexCount += 4;
        this._indexCount += 6;
    }

    /// <summary>
    /// Draws a triangle in immediate mode. The vertex order is clockwise.
    /// </summary>
    /// <param name="v1">The first vertex.</param>
    /// <param name="v2">The second vertex.</param>
    /// <param name="v3">The third vertex.</param>
    /// <param name="t1">The first texture coordinate.</param>
    /// <param name="t2">The second texture coordinate.</param>
    /// <param name="t3">The third texture coordinate.</param>
    /// <param name="color">The color to tint the triangle with.</param>
    public void DrawTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 t1, Vector2 t2, Vector2 t3, Color color)
    {
        this.Prepare(3, 3);

        this._vertices[this._vertexCount + 0] = new Vertex(v1, t1, color);
        this._vertices[this._vertexCount + 1] = new Vertex(v2, t2, color);
        this._vertices[this._vertexCount + 2] = new Vertex(v3, t3, color);

        int i = this._vertexCount;
        this._indices[this._indexCount + 0] = (ushort)(i + 0);
        this._indices[this._indexCount + 1] = (ushort)(i + 1);
        this._indices[this._indexCount + 2] = (ushort)(i + 2);

        this._vertexCount += 3;
        this._indexCount += 3;
    }

    /// <summary>
    /// Ends the immediate mode batch and flushes the remaining vertices.
    /// </summary>
    public void End()
    {
        this.Flush();
    }

    private void Flush()
    {
        if (this._vertexCount == 0 || this._indexCount == 0)
            return;

        this._commandList.UpdateBuffer(this._vertexBuffer, 0, new ReadOnlySpan<Vertex>(this._vertices, 0, this._vertexCount));
        this._commandList.UpdateBuffer(this._indexBuffer, 0, new ReadOnlySpan<ushort>(this._indices, 0, this._indexCount));

        this._commandList.SetVertexBuffer(0, this._vertexBuffer);
        this._commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);

        if (this._currentBatchShader != this._currentShaderInUse)
        {
            this._currentShaderInUse = this._currentBatchShader;
            var pipeline = this._currentShaderInUse.GetPipeline(this._vertexFormat, RenderFlags.None);
            this._commandList.SetPipeline(pipeline);
        }

        this._commandList.SetGraphicsResourceSet(0, this._passResourceSet);
        ResourceSet resourceSet = this._resourceSetCache.GetResourceSet(this._currentBatchShader, this._currentBatchTexture);
        this._commandList.SetGraphicsResourceSet(1, resourceSet);

        this._commandList.DrawIndexed((uint)this._indexCount);

        this._vertexCount = 0;
        this._indexCount = 0;
    }

    Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant, RenderFlags flags, TextureSampleCount sampleCount)
    {
        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = shaderVariant.ShaderSetDescription,
            BlendState = new BlendStateDescription(RgbaFloat.Black, BlendAttachmentDescription.OverrideBlend),
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.Back,
                PolygonFillMode.Solid,
                FrontFace.CounterClockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            ),
            Outputs = this._renderer.MainRenderTexture.Framebuffer.OutputDescription,
            ResourceLayouts = new ResourceLayout[]
            {
                this._passResourceLayout,
                shaderVariant.MaterialResourceLayout,
            },
        });
    }

    public void Dispose()
    {
        this._vertexBuffer.Dispose();
        this._indexBuffer.Dispose();
        this._passDataBuffer.Dispose();
        this._passResourceLayout.Dispose();
    }

    private static readonly string _vertexShader = @"
        #version 450
        layout(set = 0, binding = 0, std140) uniform CameraDataBuffer {
            mat4 ViewProjection;
        };

        layout(location = 0) in vec3 Position;
        layout(location = 1) in vec2 TextureCoords;
        layout(location = 2) in vec4 Color;

        layout(location = 0) out vec2 fsin_TexCoords;
        layout(location = 1) out vec4 fsin_Color;

        void main()
        {
            gl_Position = ViewProjection * vec4(Position, 1);
            fsin_TexCoords = TextureCoords;
            fsin_Color = Color;
        }
    ";

    private static readonly string _fragmentShader = @"
        #version 450
        layout(location = 0) in vec2 fsin_TexCoords;
        layout(location = 1) in vec4 fsin_Color;

        layout(set = 1, binding = 0) uniform texture2D MainTexture;
        layout(set = 1, binding = 1) uniform sampler MainSampler;

        layout(location = 0) out vec4 fsout_color;

        void main()
        {
            vec4 textureColor = texture(sampler2D(MainTexture, MainSampler), fsin_TexCoords);

            if (textureColor.a == 0)
            {
                discard;
            }

            fsout_color = textureColor * fsin_Color;
        }
    ";
}
