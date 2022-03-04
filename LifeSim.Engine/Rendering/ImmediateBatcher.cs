using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

// Draws a mesh in immediate mode trying to batch as much as possible.
public class ImmediateBatcher : IPipelineProvider, IDisposable
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

    private int _vertexCount = 0;

    private int _indexCount = 0;

    private readonly int _maxVertexBatchSize = 1024; // Number of vertices to batch before drawing. (otherwise, flush)

    private readonly int _maxIndexBatchSize = 1024; // Number of indices to batch before drawing. (otherwise, flush)

    private CommandList _commandList = null!;

    private readonly Vertex[] _vertices;

    private readonly ushort[] _indices;
    private readonly ResourceLayout _passResourceLayout;

    private readonly ResourceLayout _materialResourceLayout;
    private readonly DeviceBuffer _passDataBuffer;

    private readonly ResourceSet _passResourceSet;

    private readonly Shader _defaultShader;

    private Shader _currentBatchShader = null!;

    private Shader _currentShaderInUse = null!;

    private ITexture _currentBatchTexture = null!;

    private readonly VertexFormat _vertexFormat;

    private readonly ResourceSetCache _resourceSetCache;

    private readonly IRenderTexture _renderTexture;

    private readonly ITexture _defaultTexture;

    public ImmediateBatcher(GraphicsDevice gd, IRenderTexture renderTexture)
    {
        this._gd = gd;
        this._renderTexture = renderTexture;
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

        this._vertexFormat = new VertexFormat(
            new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)));

        this._materialResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));

        this._defaultShader = new Shader(this, _vertexShader, _fragmentShader, this._materialResourceLayout);

        this._resourceSetCache = new ResourceSetCache(factory);

        this._defaultTexture = Texture.White;
    }

    public void Begin(CommandList cl, Matrix4x4 viewProjectionMatrix)
    {
        this._commandList = cl;
        this._vertexCount = 0;
        this._indexCount = 0;
        this._currentBatchShader =  null!;
        this._currentBatchTexture = null!;
        this._currentShaderInUse = null!;

        cl.UpdateBuffer(this._passDataBuffer, 0, new PassDataBuffer { ViewProjection = viewProjectionMatrix });
    }

    private void Prepare(Shader shader, ITexture texture, int indexCount, int vertexCount)
    {
        if (this._vertexCount + vertexCount > this._maxVertexBatchSize)
        {
            this.Flush();
        }

        if (this._indexCount + indexCount > this._maxIndexBatchSize)
        {
            this.Flush();
        }

        // validate buffers overflow 
        if (this._vertexCount + vertexCount > this._maxVertexBatchSize)
        {
            throw new InvalidOperationException($"The number of provided vertices exceeds the maximum batch size. {this._vertexCount + vertexCount} > {this._maxVertexBatchSize}");
        }

        if (this._indexCount + indexCount > this._maxIndexBatchSize)
        {
            throw new InvalidOperationException($"The number of provided indices exceeds the maximum batch size. {this._indexCount + indexCount} > {this._maxIndexBatchSize}");
        }

        if (this._currentBatchShader != shader)
        {
            this.Flush();
            this._currentBatchShader = shader;
        }

        if (this._currentBatchTexture != texture)
        {
            this.Flush();
            this._currentBatchTexture = texture;
        }
    }

    public void Draw(Shader? shader, ITexture? texture, ushort[] indices, Vector3[] positions, Vector2[] texCoords, Color color)
    {
        Debug.Assert(positions.Length == texCoords.Length, "The number of positions and texture coordinates must be the same.");

        this.Prepare(shader ?? this._defaultShader, texture ?? this._defaultTexture, indices.Length, positions.Length);

        Array.Copy(indices, 0, this._indices, this._indexCount, indices.Length);

        for (int i = 0; i < positions.Length; i++)
        {
            this._vertices[this._vertexCount + i] = new Vertex(positions[i], texCoords[i], color);
        }

        this._vertexCount += positions.Length;
        this._indexCount += indices.Length;
    }

    public void End()
    {
        this.Flush();
    }

    private void Flush()
    {
        if (this._vertexCount == 0 || this._indexCount == 0)
        {
            return;
        }

        this._commandList.UpdateBuffer(this._vertexBuffer, 0, new ReadOnlySpan<Vertex>(this._vertices, 0, this._vertexCount));
        this._commandList.UpdateBuffer(this._indexBuffer, 0, new ReadOnlySpan<ushort>(this._indices, 0, this._indexCount));

        this._commandList.SetVertexBuffer(0, this._vertexBuffer);
        this._commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);

        if (this._currentBatchShader != this._currentShaderInUse)
        {
            this._currentShaderInUse = this._currentBatchShader;
            this._commandList.SetPipeline(this._currentShaderInUse.GetPipeline(this._vertexFormat));
        }

        this._commandList.SetGraphicsResourceSet(0, this._passResourceSet);
        ResourceSet resourceSet = this._resourceSetCache.GetResourceSet(this._currentBatchShader, this._currentBatchTexture);
        this._commandList.SetGraphicsResourceSet(1, resourceSet);

        this._commandList.DrawIndexed((uint)this._indexCount);

        this._vertexCount = 0;
        this._indexCount = 0;
    }

    public Pipeline MakePipeline(ShaderVariant shaderVariant)
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

        var renderTexture = (RenderTexture)this._renderTexture;

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
            Outputs = renderTexture.ColorOnlyFramebuffer.OutputDescription,
            ResourceLayouts = resources,
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
            fsout_color = textureColor * fsin_Color;
        }
    ";
}