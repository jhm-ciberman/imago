using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Core;
using Veldrid;

namespace LifeSim.Rendering
{
    public class ParticlesRenderer : IPass, IDisposable
    {
        private const int PARTICLES_PER_BATCH = 1000;


        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public Vector3 Position;
            public Vector2 TextureCoords;

            public Vertex(Vector3 position, Vector2 textureCoords)
            {
                this.Position = position;
                this.TextureCoords = textureCoords;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ParticleRenderData
        {
            public Vector3 Position;
            public float Size;
            public Color Color;
        }
        

        private int _particlesCount = 0;
        private readonly ParticleRenderData[] _particles = new ParticleRenderData[PARTICLES_PER_BATCH];
        private readonly Shader _particlesShader;
        private Shader? _currentShader = null;
        private Texture? _currentTexture = null;
        private readonly ResourceSet _passResourceSet;
        private readonly ResourceLayout _passResourceLayout;
        private readonly ResourceLayout _materialResourceLayout;
        private readonly IRenderTexture _renderTexture;
        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _particlesBuffer;

        private readonly GraphicsDevice _gd;

        private readonly CommandList _commandList;

        private readonly DeviceBuffer _viewProjectionBuffer;

        private readonly VertexFormat _vertexFormat;

        private bool _hasCommandsToSubmit = false;

        private readonly Dictionary<Texture, ResourceSet> _textures = new Dictionary<Texture, ResourceSet>();

        public ParticlesRenderer(GraphicsDevice gd, IRenderTexture renderTexture)
        {
            this._renderTexture = renderTexture;
            this._gd = gd;
            var factory = gd.ResourceFactory;

            this._vertexBuffer = factory.CreateBuffer(new BufferDescription((uint) (4 * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
            this._particlesBuffer = factory.CreateBuffer(new BufferDescription((uint) (PARTICLES_PER_BATCH * Marshal.SizeOf<ParticleRenderData>()), BufferUsage.VertexBuffer | BufferUsage.Dynamic));

            this._viewProjectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            var quad = new Vertex[] {
                new Vertex(new Vector3(-0.5f, -0.5f, 0), new Vector2(0f, 1f)),
                new Vertex(new Vector3(0.5f, -0.5f, 0), new Vector2(1f, 1f)),
                new Vertex(new Vector3(-0.5f, 0.5f, 0), new Vector2(0f, 0f)),
                new Vertex(new Vector3(0.5f, 0.5f, 0), new Vector2(1f, 0f)),
            };

            gd.UpdateBuffer(this._vertexBuffer, 0, quad);

            this._passResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            this._materialResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)
            ));


            this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._passResourceLayout, this._viewProjectionBuffer));
            Console.WriteLine("Marshal.SizeOf<ParticleRenderData>(): " + Marshal.SizeOf<ParticleRenderData>());
            this._vertexFormat = new VertexFormat(
                new VertexLayoutDescription(
                    new VertexElementDescription("Position", VertexElementSemantic.Position, VertexElementFormat.Float3),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                ),
                new VertexLayoutDescription(stride: (uint)Marshal.SizeOf<ParticleRenderData>(), instanceStepRate: 1,
                    new VertexElementDescription("PositionSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
                )
            );

            this._particlesShader = new Shader(this, new ShaderSource("particles.vert.glsl", "particles.frag.glsl"), this._materialResourceLayout);
            this._commandList = factory.CreateCommandList();
        }

        public void Render(IReadOnlyList<Particle> particles, Texture texture, ICamera camera)
        {
            this._currentShader = null;
            this._currentTexture = null;

            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._renderTexture.Framebuffer);

            var viewProjectionMatrix = camera.ViewProjectionMatrix;
            this._commandList.UpdateBuffer(this._viewProjectionBuffer, 0, ref viewProjectionMatrix);

            this._RenderParticles(particles, texture);

            this._commandList.End();
            this._hasCommandsToSubmit = true;
        }

        public void Submit()
        {
            if (! this._hasCommandsToSubmit) return;
            this._gd.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }

        private void _RenderParticles(IReadOnlyList<Particle> particles, Texture texture)
        {
            this._particlesCount = 0;

            for (var i = 0; i < particles.Count; i++) {
                var particle = particles[i];
                if (particle.Life <= 0) continue;

                if (this._particlesCount + 1 >= PARTICLES_PER_BATCH) {
                    this._FlushParticles(this._particlesShader, texture);
                }

                this._particles[this._particlesCount++] = new ParticleRenderData {
                    Position = particle.Position,
                    Size = particle.Size,
                    Color = particle.Color,
                };
            }

            if (this._particlesCount > 0) {
                this._FlushParticles(this._particlesShader, texture);
            }
        }

        private void _FlushParticles(Shader shader, Texture texture)
        {
            this._commandList.UpdateBuffer(this._particlesBuffer, 0, new ReadOnlySpan<ParticleRenderData>(this._particles, 0, this._particlesCount));
            Console.WriteLine($"Flushing {this._particlesCount} particles");

            if (this._currentShader != shader) {
                this._currentShader = shader;
                var pipeline = shader.GetPipeline(this._vertexFormat);
                this._commandList.SetPipeline(pipeline);

                this._commandList.SetGraphicsResourceSet(0, this._passResourceSet);
            }

            if (this._currentTexture != texture) {
                this._currentTexture = texture;
                var resourceSet = this._GetTextureResourceSet(texture);
                this._commandList.SetGraphicsResourceSet(1, resourceSet);
            }

            this._commandList.SetVertexBuffer(0, this._vertexBuffer);
            this._commandList.SetVertexBuffer(1, this._particlesBuffer);
            this._commandList.Draw(4, (uint)this._particlesCount, 0, 0);
            this._particlesCount = 0;
        }

        private ResourceSet _GetTextureResourceSet(Texture texture)
        {
            if (this._textures.TryGetValue(texture, out var resourceSet)) {
                return resourceSet;
            }

            resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._materialResourceLayout, texture.Resource, texture.Sampler));
            this._textures.Add(texture, resourceSet);
            return resourceSet;
        }

        Pipeline IPass.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );

            Debug.Assert(shaderVariant.MaterialResourceLayout != null);

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription() {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.TriangleStrip,
                ShaderSet = shaderVariant.ShaderSetDescription,
                BlendState = BlendStateDescription.SingleAlphaBlend,
                RasterizerState = rasterizerState,
                Outputs = this._renderTexture.OutputDescription,
                ResourceLayouts = new ResourceLayout[] {
                    this._passResourceLayout,
                    shaderVariant.MaterialResourceLayout,
                },
            });
        }

        public void Dispose()
        {
            this._commandList.Dispose();
            this._vertexBuffer.Dispose();
            this._viewProjectionBuffer.Dispose();
            this._passResourceLayout.Dispose();
            this._passResourceSet.Dispose();
            this._particlesShader.Dispose();
        }
    }
}