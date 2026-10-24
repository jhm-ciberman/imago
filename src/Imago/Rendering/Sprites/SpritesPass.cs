using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Assets.Materials;
using Imago.Assets.Textures;
using Imago.Rendering.Internals;
using Imago.Support.Numerics;
using NeoVeldrid;

namespace Imago.Rendering.Sprites;

/// <summary>
/// Draws sprite quads into a render target. Quads that share a texture are batched into one draw call.
/// </summary>
internal class SpritesPass : IDisposable
{
    /// <summary>
    /// Gets the sprite batch that draws with this pass.
    /// </summary>
    public SpriteBatch SpriteBatch { get; }

    private readonly GraphicsDevice _gd;

    private readonly NeoVeldrid.Shader[] _shaders;

    private readonly OutputDescription _outputDescription;

    private readonly bool _depthTested;

    private readonly RenderStatistics _statistics;

    private readonly SpriteQuad[] _quads;

    private readonly VertexLayoutDescription _vertexLayout;

    private readonly DeviceBuffer _vertexBuffer;

    private readonly DeviceBuffer _indexBuffer;

    private readonly DeviceBuffer _matrixBuffer;

    private readonly ResourceLayout _passResourceLayout;

    private readonly ResourceSet _passResourceSet;

    private readonly ResourceLayout _textureResourceLayout;

    private readonly ResourceSetCache _resourceSetCache;

    private readonly Pipeline _pipeline;

    private readonly Pipeline _scissorPipeline;

    private int _quadCount;

    private ITexture? _texture;

    private bool _isScissorEnabled;

    private CommandList _commandList = null!;

    private Pipeline? _currentPipeline;

    private int _drawCallCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpritesPass"/> class.
    /// </summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="outputTarget">The render texture whose output description determines pipeline compatibility. If null, uses the main render texture.</param>
    /// <param name="capacity">The maximum number of sprites that can be batched in a single draw call.</param>
    /// <param name="depthTested">Whether sprites test and write depth. Pass false for overlays that must draw on top of everything regardless of the target's depth buffer.</param>
    public SpritesPass(Renderer renderer, IRenderTexture? outputTarget = null, int capacity = 1000, bool depthTested = true)
    {
        this._gd = renderer.GraphicsDevice;
        this._outputDescription = (outputTarget ?? renderer.MainRenderTexture).OutputDescription;
        this._depthTested = depthTested;
        this._statistics = renderer.Statistics;
        this._shaders = ShaderCompiler.CompileShaders(this._gd, _vertexShader, _fragmentShader);
        var factory = this._gd.ResourceFactory;

        var vertexBufferSize = (uint) (Marshal.SizeOf<SpriteQuad>() * capacity);
        this._vertexBuffer = factory.CreateBuffer(new BufferDescription(vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));

        var indexBufferSize = (uint) (sizeof(ushort) * 6 * capacity);
        this._indexBuffer = factory.CreateBuffer(new BufferDescription(indexBufferSize, BufferUsage.IndexBuffer));
        ushort[] indices = new ushort[capacity * 6];

        for (int i = 0; i < capacity; i++)
        {
            int j = i * 6;
            int offset = i * 4;
            indices[j + 0] = (ushort)(offset + 0);
            indices[j + 1] = (ushort)(offset + 2);
            indices[j + 2] = (ushort)(offset + 1);
            indices[j + 3] = (ushort)(offset + 0);
            indices[j + 4] = (ushort)(offset + 3);
            indices[j + 5] = (ushort)(offset + 2);
        }

        this._gd.UpdateBuffer(this._indexBuffer, 0, indices);

        this._quads = new SpriteQuad[capacity];

        this._vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
        );

        this._passResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._matrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._passResourceLayout, this._matrixBuffer));

        this._textureResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));

        this._resourceSetCache = new ResourceSetCache(factory);

        this._pipeline = this.CreatePipeline(isScissorEnabled: false);
        this._scissorPipeline = this.CreatePipeline(isScissorEnabled: true);

        this.SpriteBatch = new SpriteBatch(this._gd, this);
    }

    /// <summary>
    /// Disposes of the resources used by this <see cref="SpritesPass"/>.
    /// </summary>
    public void Dispose()
    {
        this._indexBuffer.Dispose();
        this._vertexBuffer.Dispose();
        this._matrixBuffer.Dispose();
        this._passResourceLayout.Dispose();
        this._passResourceSet.Dispose();
        this._textureResourceLayout.Dispose();
        this._resourceSetCache.Dispose();
        this._pipeline.Dispose();
        this._scissorPipeline.Dispose();

        foreach (var shader in this._shaders)
        {
            shader.Dispose();
        }
    }

    /// <summary>
    /// Begins recording quads on the given command list, drawing into the given target.
    /// </summary>
    /// <param name="cl">The command list to record on.</param>
    /// <param name="target">The render texture to draw into.</param>
    public void Begin(CommandList cl, IRenderTexture target)
    {
        cl.SetFramebuffer(target.Framebuffer);
        cl.SetFullViewports();
        cl.SetFullScissorRects();

        this._commandList = cl;
        this._quadCount = 0;
        this._isScissorEnabled = false;
        this._currentPipeline = null;
        this._drawCallCount = 0;
    }

    /// <summary>
    /// Sets the view projection matrix for the next quads.
    /// </summary>
    /// <param name="viewProjectionMatrix">The view projection matrix to use.</param>
    public void SetViewProjectionMatrix(Matrix4x4 viewProjectionMatrix)
    {
        this._commandList.UpdateBuffer(this._matrixBuffer, 0, ref viewProjectionMatrix);
    }

    /// <summary>
    /// Flushes the remaining quads and ends recording.
    /// </summary>
    public void End()
    {
        this.Flush();
        this._statistics.CurrentGuiDrawCalls += this._drawCallCount;
    }

    /// <summary>
    /// Draws a textured quad.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="quad">The quad to draw.</param>
    public void Draw(ITexture texture, in SpriteQuad quad)
    {
        this.Reserve(texture, 1);
        this._quads[this._quadCount++] = quad;
    }

    /// <summary>
    /// Draws textured quads that share the given texture. Quads that fit in the current batch are drawn in one
    /// draw call.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="quads">The quads to draw.</param>
    public void Draw(ITexture texture, ReadOnlySpan<SpriteQuad> quads)
    {
        while (quads.Length > 0)
        {
            int count = int.Min(quads.Length, this._quads.Length);
            this.Reserve(texture, count);
            quads[..count].CopyTo(this._quads.AsSpan(this._quadCount));
            this._quadCount += count;
            quads = quads[count..];
        }
    }

    /// <summary>
    /// Clips the next quads to the given rectangle.
    /// </summary>
    /// <param name="rect">The rectangle to clip to.</param>
    public void SetScissorRectangle(Rect rect)
    {
        this.Flush();
        this._isScissorEnabled = true;
        this._commandList.SetScissorRect(0, (uint)rect.X, (uint)rect.Y, (uint)rect.Width, (uint)rect.Height);
    }

    /// <summary>
    /// Stops clipping the next quads.
    /// </summary>
    public void ClearScissorRectangle()
    {
        this.Flush();
        this._isScissorEnabled = false;
        this._commandList.SetFullScissorRect(0);
    }

    private Pipeline CreatePipeline(bool isScissorEnabled)
    {
        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = (this._depthTested && this._outputDescription.DepthAttachment.HasValue)
                ? DepthStencilStateDescription.DepthOnlyLessEqual
                : DepthStencilStateDescription.Disabled,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription([this._vertexLayout], this._shaders),
            BlendState = GetBlendState(),
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.Back,
                PolygonFillMode.Solid,
                FrontFace.CounterClockwise,
                depthClipEnabled: true,
                isScissorEnabled
            ),
            Outputs = this._outputDescription,
            ResourceLayouts =
            [
                this._passResourceLayout,
                this._textureResourceLayout,
            ],
        });
    }

    private void Reserve(ITexture texture, int quadCount)
    {
        if (this._texture == texture)
        {
            if (this._quadCount + quadCount > this._quads.Length)
                this.Flush();
            return;
        }

        this.Flush();
        this._texture = texture;
    }

    private void Flush()
    {
        if (this._quadCount == 0 || this._texture == null)
            return;

        this._commandList.UpdateBuffer(this._vertexBuffer, 0, this._quads.AsSpan(0, this._quadCount));

        var pipeline = this._isScissorEnabled ? this._scissorPipeline : this._pipeline;
        if (pipeline != this._currentPipeline)
        {
            this._currentPipeline = pipeline;
            this._commandList.SetPipeline(pipeline);
            this._commandList.SetGraphicsResourceSet(0, this._passResourceSet);
        }

        this._commandList.SetVertexBuffer(0, this._vertexBuffer);
        this._commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);

        var resourceSet = this._resourceSetCache.GetResourceSet(this._textureResourceLayout, this._texture);
        this._commandList.SetGraphicsResourceSet(1, resourceSet);
        this._commandList.DrawIndexed((uint)this._quadCount * 6);

        this._quadCount = 0;
        this._drawCallCount++;
    }

    private static BlendStateDescription GetBlendState()
    {
        // Our target is transparent, not opaque, so we cannot use the classic BlendStateDescription.SingleAlphaBlend, because that
        // only works when the output is opaque. But here we are rendering the UI to a transparent target.
        // See: https://limnu.com/webgl-blending-youre-probably-wrong/
        // So we use the formula:
        // out.rgb = src.rgb * src.a + dst.rgb * (1 - src.a)
        // out.a   = src.a * 1 + dst.a * (1 - src.a)
        return new BlendStateDescription
        {
            AttachmentStates = [
                new BlendAttachmentDescription
                {
                    BlendEnabled = true,
                    SourceColorFactor = BlendFactor.SourceAlpha,
                    DestinationColorFactor = BlendFactor.InverseSourceAlpha,
                    ColorFunction = BlendFunction.Add,
                    SourceAlphaFactor = BlendFactor.One,
                    DestinationAlphaFactor = BlendFactor.InverseSourceAlpha,
                    AlphaFunction = BlendFunction.Add
                }
            ],
        };
    }

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
}
