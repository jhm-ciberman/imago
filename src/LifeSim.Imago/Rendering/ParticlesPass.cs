using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Imago.Materials;
using LifeSim.Imago.SceneGraph;
using LifeSim.Imago.SceneGraph.Cameras;
using LifeSim.Imago.Textures;
using LifeSim.Support.Drawing;
using Veldrid;

namespace LifeSim.Imago.Rendering;

internal class ParticlesPass : IDisposable
{
    private const int PARTICLES_PER_BATCH = 1000;


    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        public Vector2 VertexPosition;
        public Vector2 TextureCoords;

        public Vertex(Vector2 vertexPosition, Vector2 textureCoords)
        {
            this.VertexPosition = vertexPosition;
            this.TextureCoords = textureCoords;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct CameraDataBuffer
    {
        public Matrix4x4 ViewProjection;
        public Vector3 CameraRight;
        private readonly float _padding;
        public Vector3 CameraUp;
        private readonly float _padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ParticleRenderData
    {
        public Vector4 PositionSize;
        public Color Color;

        public ParticleRenderData(Vector3 position, float size, Color color)
        {
            this.PositionSize = new Vector4(position, size);
            this.Color = color;
        }
    }

    private ITexture? _currentTexture = null;
    private readonly ResourceSet _passResourceSet;
    private readonly ResourceLayout _passResourceLayout;
    private readonly ResourceLayout _materialResourceLayout;
    private readonly DeviceBuffer _vertexBuffer;
    private readonly DeviceBuffer _particlesBuffer;

    private readonly GraphicsDevice _gd;

    private readonly DeviceBuffer _viewProjectionBuffer;

    private readonly Pipeline _pipeline;

    private readonly ParticleRenderData[] _particlesForRender = new ParticleRenderData[PARTICLES_PER_BATCH];

    private readonly Dictionary<ITexture, ResourceSet> _textures = new Dictionary<ITexture, ResourceSet>();

    public ParticlesPass(Renderer renderer)
    {
        this._particlesForRender = new ParticleRenderData[PARTICLES_PER_BATCH];
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(4 * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
        this._particlesBuffer = factory.CreateBuffer(new BufferDescription((uint)(PARTICLES_PER_BATCH * Marshal.SizeOf<ParticleRenderData>()), BufferUsage.VertexBuffer | BufferUsage.Dynamic));

        this._viewProjectionBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<CameraDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        var quad = new Vertex[] {
            new Vertex(new Vector2(-0.5f, -0.5f), new Vector2(0f, 1f)),
            new Vertex(new Vector2(0.5f, -0.5f), new Vector2(1f, 1f)),
            new Vertex(new Vector2(-0.5f, 0.5f), new Vector2(0f, 0f)),
            new Vertex(new Vector2(0.5f, 0.5f), new Vector2(1f, 0f)),
        };

        this._gd.UpdateBuffer(this._vertexBuffer, 0, quad);

        this._passResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._materialResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
            new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
        ));


        this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._passResourceLayout, this._viewProjectionBuffer));

        var vertexLayouts = new[]
        {
            new VertexLayoutDescription(
                new VertexElementDescription("VertexPosition", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
            ),
            new VertexLayoutDescription(stride: (uint)Marshal.SizeOf<ParticleRenderData>(), instanceStepRate: 1,
                new VertexElementDescription("PositionSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
            )
        };

        var shaders = ShaderCompiler.CompileShaders(this._gd, _vertexShader, _fragmentShader);

        this._pipeline = this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
            PrimitiveTopology = PrimitiveTopology.TriangleStrip,
            ShaderSet = new ShaderSetDescription(vertexLayouts, shaders),
            BlendState = BlendStateDescription.SingleAlphaBlend,
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            ),
            Outputs = renderer.MainRenderTexture.OutputDescription,
            ResourceLayouts = new ResourceLayout[]
            {
                this._passResourceLayout,
                this._materialResourceLayout,
            },
        });
    }

    /// <summary>
    /// Renders the specified particle systems to the render texture.
    /// </summary>
    /// <param name="cl">The command list to use.</param>
    /// <param name="renderTexture">The render texture to render to.</param>
    /// <param name="camera">The camera to use.</param>
    /// <param name="particleSystems">The particle systems to render.</param>
    public void Render(CommandList cl, RenderTexture renderTexture, Camera camera, IReadOnlyList<IParticleSystem> particleSystems)
    {
        this._currentTexture = null;

        cl.SetFramebuffer(renderTexture.Framebuffer);
        cl.SetPipeline(this._pipeline);
        cl.SetGraphicsResourceSet(0, this._passResourceSet);

        for (int i = 0; i < particleSystems.Count; i++)
        {
            IParticleSystem system = particleSystems[i];

            cl.UpdateBuffer(this._viewProjectionBuffer, 0, new CameraDataBuffer
            {
                ViewProjection = camera.ViewProjectionMatrix,
                CameraRight = camera.Right,
                CameraUp = camera.Up
            });

            system.SortParticles(camera.Position);

            this.RenderParticles(cl, system.Particles, system.Texture);
        }
    }

    private void RenderParticles(CommandList cl, IReadOnlyList<Particle> particles, ITexture texture)
    {
        if (particles.Count == 0) return;
        if (particles.Count <= PARTICLES_PER_BATCH)
            this.FlushParticles(cl, particles, 0, particles.Count, texture);

        int batchStartIndex = 0;
        int batchEndIndex;
        int numberOfbatches = (int)Math.Ceiling(particles.Count / (float)PARTICLES_PER_BATCH);

        for (int i = 0; i < numberOfbatches; i++)
        {
            batchEndIndex = Math.Min(particles.Count, batchStartIndex + PARTICLES_PER_BATCH);
            this.FlushParticles(cl, particles, batchStartIndex, batchEndIndex, texture);
            batchStartIndex = batchEndIndex;
        }
    }

    private void FlushParticles(CommandList cl, IReadOnlyList<Particle> particles, int startIndex, int endIndex, ITexture texture)
    {
        int particlesCount = 0;
        for (int i = startIndex; i < endIndex; i++)
        {
            var particle = particles[i];
            this._particlesForRender[particlesCount++] = new ParticleRenderData(particle.Position, particle.Size, particle.Color);
        }

        cl.UpdateBuffer(this._particlesBuffer, 0, this._particlesForRender);

        if (this._currentTexture != texture)
        {
            this._currentTexture = texture;
            var resourceSet = this.GetTextureResourceSet(texture);
            cl.SetGraphicsResourceSet(1, resourceSet);
        }

        cl.SetVertexBuffer(0, this._vertexBuffer);
        cl.SetVertexBuffer(1, this._particlesBuffer);
        cl.Draw(4, (uint)particlesCount, 0, 0);
    }

    private ResourceSet GetTextureResourceSet(ITexture texture)
    {
        if (this._textures.TryGetValue(texture, out var resourceSet))
            return resourceSet;

        resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._materialResourceLayout, texture.VeldridTexture, texture.VeldridSampler));
        this._textures.Add(texture, resourceSet);
        return resourceSet;
    }

    public void Dispose()
    {
        this._vertexBuffer.Dispose();
        this._viewProjectionBuffer.Dispose();
        this._passResourceLayout.Dispose();
        this._passResourceSet.Dispose();
        this._pipeline.Dispose();
    }

    private static readonly string _vertexShader = @"
        #version 450
        layout(set = 0, binding = 0, std140) uniform CameraDataBuffer {
            mat4 ViewProjection;
            vec3 CameraRight;
            vec3 CameraUp;
        };

        layout(location = 0) in vec2 VertexPosition;
        layout(location = 1) in vec2 TextureCoords;

        layout(location = 2) in vec4 PositionSize; // xyz = Position, w = Size
        layout(location = 3) in vec4 Color;

        layout(location = 0) out vec2 fsin_TexCoords;
        layout(location = 1) out vec4 fsin_Color;

        void main()
        {
            float size = PositionSize.w;
            vec3 center = PositionSize.xyz;

            vec3 worldPos = center
                + CameraRight * VertexPosition.x * size
                + CameraUp * VertexPosition.y * size;

            gl_Position = ViewProjection * vec4(worldPos, 1.0);

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

            if (textureColor.a < 0.01) discard;

            fsout_color = textureColor * fsin_Color;
        }
        ";
}
