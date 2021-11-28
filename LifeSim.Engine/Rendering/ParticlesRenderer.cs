using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class ParticlesPass : IPipelineProvider, IDisposable
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

        private readonly ParticleRenderData[] _particlesForRender = new ParticleRenderData[PARTICLES_PER_BATCH];

        private bool _hasCommandsToSubmit = false;

        private readonly Dictionary<Texture, ResourceSet> _textures = new Dictionary<Texture, ResourceSet>();

        public ParticlesPass(GraphicsDevice gd, IRenderTexture renderTexture)
        {
            this._renderTexture = renderTexture;
            this._particlesForRender = new ParticleRenderData[PARTICLES_PER_BATCH];
            this._gd = gd;
            var factory = gd.ResourceFactory;

            this._vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(4 * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer));
            this._particlesBuffer = factory.CreateBuffer(new BufferDescription((uint)(PARTICLES_PER_BATCH * Marshal.SizeOf<ParticleRenderData>()), BufferUsage.VertexBuffer | BufferUsage.Dynamic));

            this._viewProjectionBuffer = factory.CreateBuffer(new BufferDescription((uint)Marshal.SizeOf<CameraDataBuffer>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            var quad = new Vertex[] {
                new Vertex(new Vector2(-0.5f, -0.5f), new Vector2(0f, 1f)),
                new Vertex(new Vector2(0.5f, -0.5f), new Vector2(1f, 1f)),
                new Vertex(new Vector2(-0.5f, 0.5f), new Vector2(0f, 0f)),
                new Vertex(new Vector2(0.5f, 0.5f), new Vector2(1f, 0f)),
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

            this._vertexFormat = new VertexFormat(
                new VertexLayoutDescription(
                    new VertexElementDescription("VertexPosition", VertexElementSemantic.Position, VertexElementFormat.Float2),
                    new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)
                ),
                new VertexLayoutDescription(stride: (uint)Marshal.SizeOf<ParticleRenderData>(), instanceStepRate: 1,
                    new VertexElementDescription("PositionSize", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
                    new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
                )
            );

            var vertex = ShaderSource.Load("particles.vert.glsl");
            var fragment = ShaderSource.Load("particles.frag.glsl");
            this._particlesShader = new Shader(this, vertex, fragment, this._materialResourceLayout);
            this._commandList = factory.CreateCommandList();
            this._commandList.Name = "Particles Renderer";
        }



        public void Render(IReadOnlyList<Particle> particles, Texture texture, ICamera camera)
        {
            this._currentShader = null;
            this._currentTexture = null;

            this._commandList.Begin();
            this._commandList.SetFramebuffer(this._renderTexture.Framebuffer);

            this._commandList.UpdateBuffer(this._viewProjectionBuffer, 0, new CameraDataBuffer
            {
                ViewProjection = camera.ViewProjectionMatrix,
                CameraRight = camera.Right,
                CameraUp = camera.Up
            });

            this._RenderParticles(particles, this._particlesShader, texture);

            this._commandList.End();
            this._hasCommandsToSubmit = true;
        }

        public void Submit()
        {
            if (!this._hasCommandsToSubmit) return;
            this._gd.SubmitCommands(this._commandList);
            this._hasCommandsToSubmit = false;
        }

        private void _RenderParticles(IReadOnlyList<Particle> particles, Shader shader, Texture texture)
        {
            if (particles.Count == 0) return;
            if (particles.Count <= PARTICLES_PER_BATCH)
            {
                this._FlushParticles(particles, 0, particles.Count, shader, texture);
            }

            int batchStartIndex = 0;
            int batchEndIndex;
            int numberOfbatches = (int)Math.Ceiling(particles.Count / (float)PARTICLES_PER_BATCH);

            for (int i = 0; i < numberOfbatches; i++)
            {
                batchEndIndex = Math.Min(particles.Count, batchStartIndex + PARTICLES_PER_BATCH);
                this._FlushParticles(particles, batchStartIndex, batchEndIndex, shader, texture);
                batchStartIndex = batchEndIndex;
            }
        }

        private void _FlushParticles(IReadOnlyList<Particle> particles, int startIndex, int endIndex, Shader shader, Texture texture)
        {
            int particlesCount = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                var particle = particles[i];
                this._particlesForRender[particlesCount++] = new ParticleRenderData(particle.Position, particle.Size, particle.Color);
            }

            this._commandList.UpdateBuffer(this._particlesBuffer, 0, this._particlesForRender);

            if (this._currentShader != shader)
            {
                this._currentShader = shader;
                var pipeline = shader.GetPipeline(this._vertexFormat);
                this._commandList.SetPipeline(pipeline);

                this._commandList.SetGraphicsResourceSet(0, this._passResourceSet);
            }

            if (this._currentTexture != texture)
            {
                this._currentTexture = texture;
                var resourceSet = this._GetTextureResourceSet(texture);
                this._commandList.SetGraphicsResourceSet(1, resourceSet);
            }

            this._commandList.SetVertexBuffer(0, this._vertexBuffer);
            this._commandList.SetVertexBuffer(1, this._particlesBuffer);
            this._commandList.Draw(4, (uint)particlesCount, 0, 0);
        }

        private ResourceSet _GetTextureResourceSet(Texture texture)
        {
            if (this._textures.TryGetValue(texture, out var resourceSet))
            {
                return resourceSet;
            }

            resourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(this._materialResourceLayout, texture.DeviceTexture, texture.Sampler));
            this._textures.Add(texture, resourceSet);
            return resourceSet;
        }

        Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Solid,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );

            Debug.Assert(shaderVariant.MaterialResourceLayout != null);

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
            {
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