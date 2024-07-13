using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Textures;
using LifeSim.Imago.SceneGraph;
using LifeSim.Imago.SceneGraph.Cameras;
using Veldrid;

namespace LifeSim.Imago.Graphics.Rendering;

internal class SkyDomePass : IDisposable
{
    private struct Vertex
    {
        public Vector3 Position;
    }

    private struct PassData
    {
        public Matrix4x4 ViewProjectionMatrix { get; set; }
        public Matrix4x4 ModelMatrix { get; set; }
        public Vector2 LutTextureOffset { get; set; }
        public Vector2 Padding0 { get; set; }
    }

    private readonly GraphicsDevice _gd;

    private readonly DeviceBuffer _vertexBuffer;

    private readonly DeviceBuffer _indexBuffer;

    private readonly int _indexCount;

    private readonly ResourceLayout _resourceLayout;

    private readonly DeviceBuffer _passDataBuffer;

    private readonly Pipeline _pipeline;

    private readonly Dictionary<ITexture, ResourceSet> _resourceSetCache = new();

    private readonly Sampler _sampler;

    public SkyDomePass(Renderer renderer)
    {
        this._gd = renderer.GraphicsDevice;

        var factory = this._gd.ResourceFactory;

        int subdivisions = 40;

        var (vertices, indices) = MakeSphereMesh(subdivisions);

        this._vertexBuffer = factory.CreateBuffer(new BufferDescription(
            (uint)(Marshal.SizeOf<Vertex>() * vertices.Length), BufferUsage.VertexBuffer));

        this._indexBuffer = factory.CreateBuffer(new BufferDescription(
            (uint)(Marshal.SizeOf<ushort>() * indices.Length), BufferUsage.IndexBuffer));

        this._gd.UpdateBuffer(this._vertexBuffer, 0, vertices);
        this._gd.UpdateBuffer(this._indexBuffer, 0, indices);

        this._indexCount = indices.Length;

        var vertexFormat = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3)
        );

        this._resourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("PassDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("LutTexture", ResourceKind.TextureReadOnly, ShaderStages.Vertex),
            new ResourceLayoutElementDescription("LutSampler", ResourceKind.Sampler, ShaderStages.Vertex)
        ));

        this._passDataBuffer = factory.CreateBuffer(new BufferDescription(
            (uint)Marshal.SizeOf<PassData>(), BufferUsage.UniformBuffer));

        var shaders = ShaderCompiler.CompileShaders(this._gd, _vertexCode, _fragmentCode);
        this._pipeline = this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            ShaderSet = new ShaderSetDescription(new[] { vertexFormat }, shaders),
            BlendState = BlendStateDescription.SingleOverrideBlend,
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.Front,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            ),
            Outputs = renderer.MainRenderTexture.OutputDescription,
            ResourceLayouts = new ResourceLayout[] { this._resourceLayout },
        });

        this._sampler = factory.CreateSampler(new SamplerDescription(
            SamplerAddressMode.Clamp, SamplerAddressMode.Clamp, SamplerAddressMode.Clamp,
            SamplerFilter.MinLinear_MagLinear_MipLinear,
            comparisonKind: null,
            0, 0, 0, 0, SamplerBorderColor.OpaqueBlack));
    }

    private ResourceSet GetResourceSet(ITexture texture)
    {
        if (!this._resourceSetCache.TryGetValue(texture, out var resourceSet))
        {
            resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
                this._resourceLayout,
                this._passDataBuffer,
                texture.VeldridTexture,
                this._sampler
            ));

            this._resourceSetCache.Add(texture, resourceSet);
        }

        return resourceSet;
    }

    public void Dispose()
    {
        foreach (var resourceSet in this._resourceSetCache.Values)
        {
            resourceSet.Dispose();
        }

        this._sampler.Dispose();
        this._passDataBuffer.Dispose();
        this._resourceLayout.Dispose();
        this._pipeline.Dispose();
        this._indexBuffer.Dispose();
        this._vertexBuffer.Dispose();
    }

    /// <summary>
    /// Renders the sky dome.
    /// </summary>
    /// <param name="cl">The command list to use.</param>
    /// <param name="renderTexture">The target render texture.</param>
    /// <param name="camera">The camera.</param>
    /// <param name="environment">The scene environment settings.</param>
    public void Render(CommandList cl, RenderTexture renderTexture, Camera camera, SceneEnvironment environment)
    {
        if (environment.SkyDomeLut == null) return;

        var xOffset = environment.SkyDomeDayProgress % 1f;

        var passData = new PassData
        {
            ViewProjectionMatrix = camera.ViewProjectionMatrix,
            ModelMatrix = Matrix4x4.CreateScale(camera.FarPlane - 0.1f) * Matrix4x4.CreateTranslation(camera.Position),
            LutTextureOffset = new Vector2(xOffset, 0.75f / 24f),
        };

        cl.SetFramebuffer(renderTexture.Framebuffer);
        cl.SetFullViewports();
        cl.SetPipeline(this._pipeline);

        cl.SetVertexBuffer(0, this._vertexBuffer);
        cl.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);

        var resourceSet = this.GetResourceSet(environment.SkyDomeLut);
        cl.SetGraphicsResourceSet(0, resourceSet);

        cl.UpdateBuffer(this._passDataBuffer, 0, ref passData);

        cl.DrawIndexed((uint)this._indexCount, 1, 0, 0, 0);
    }

    private static (Vertex[] Vertices, ushort[] Indices) MakeSphereMesh(int subdivisions)
    {
        var vertices = new List<Vertex>();
        var indices = new List<ushort>();

        float radius = 1f;
        float t = MathF.PI * 2f / subdivisions;
        for (var i = 0; i < subdivisions + 2; i++)
        {
            float theta = i * t;
            float sinTheta = MathF.Sin(theta);
            float cosTheta = MathF.Cos(theta);
            for (var j = 0; j < subdivisions; j++)
            {
                float phi = j * t;
                float sinPhi = MathF.Sin(phi);
                float cosPhi = MathF.Cos(phi);
                float x = cosPhi * sinTheta;
                float y = cosTheta;
                float z = sinPhi * sinTheta;
                //var u = 1f - (float)j / subdivisions;
                //var v = 1f - (float)i / subdivisions;
                vertices.Add(new Vertex
                {
                    Position = new Vector3(x * radius, y * radius, z * radius),
                });
            }
        }

        for (var i = 0; i < subdivisions + 1; i++)
        {
            for (var j = 0; j < subdivisions; j++)
            {
                var i0 = i * subdivisions + j;
                var i1 = i0 + 1;
                var i2 = i0 + subdivisions;
                var i3 = i2 + 1;
                indices.Add((ushort)i0);
                indices.Add((ushort)i1);
                indices.Add((ushort)i2);
                indices.Add((ushort)i1);
                indices.Add((ushort)i3);
                indices.Add((ushort)i2);
            }
        }

        return (vertices.ToArray(), indices.ToArray());
    }

    private static readonly string _vertexCode = @"
        #version 450
        layout(location = 0) in vec3 Position;

        layout(set = 0, binding = 0) uniform PassDataBuffer
        {
            mat4 ViewProjectionMatrix;
            mat4 ModelMatrix;
            vec2 LutTextureOffset; // x: offset, y: east/west delta
        } passData;

        layout(set = 0, binding = 1) uniform texture2D LutTexture;
        layout(set = 0, binding = 2) uniform sampler LutSampler;

        layout(location = 0) out vec4 fsin_Color;

        void main()
        {
            gl_Position = passData.ViewProjectionMatrix * passData.ModelMatrix * vec4(Position, 1.0);

            float eastWestDelta = Position.x * passData.LutTextureOffset.y;

            vec2 texCoord = vec2(passData.LutTextureOffset.x - eastWestDelta, 1.0 - Position.y);

            fsin_Color = texture(sampler2D(LutTexture, LutSampler), texCoord);
        }
    ";

    private static readonly string _fragmentCode = @"
        #version 450
        layout(location = 0) in vec4 fsin_Color;
        layout(location = 0) out vec4 fsout_color;
        void main()
        {
            fsout_color = fsin_Color;
        }
    ";
}
