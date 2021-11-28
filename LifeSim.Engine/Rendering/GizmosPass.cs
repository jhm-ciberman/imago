using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Engine.Rendering
{
    public class GizmosPass : IPipelineProvider, IDisposable
    {
        private const int VERTICES_PER_BATCH = 1000;



        [StructLayout(LayoutKind.Sequential)]
        private struct Vertex
        {
            public Vector3 Position;
            public uint Color;
        }


        private int _verticesCount = 0;
        private readonly Vertex[] _vertices = new Vertex[VERTICES_PER_BATCH];
        private readonly Shader _lineShader;
        private Shader? _currentShader = null;
        private readonly ResourceSet _passResourceSet;
        private readonly ResourceLayout _passResourceLayout;
        private readonly IRenderTexture _renderTexture;
        private readonly DeviceBuffer _vertexBuffer;

        private readonly GraphicsDevice _gd;

        private readonly DeviceBuffer _viewProjectionBuffer;

        private readonly VertexFormat _vertexFormat;

        public GizmosPass(GraphicsDevice gd, IRenderTexture renderTexture)
        {
            this._renderTexture = renderTexture;
            this._gd = gd;
            var factory = gd.ResourceFactory;

            this._vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VERTICES_PER_BATCH * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            this._viewProjectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._passResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
            ));

            this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._passResourceLayout, this._viewProjectionBuffer));

            this._vertexFormat = new VertexFormat(new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
            ));

            var vertex = ShaderSource.Load("lines.vert.glsl");
            var fragment = ShaderSource.Load("lines.frag.glsl");
            this._lineShader = new Shader(this, vertex, fragment);
        }

        public void Render(CommandList cl, IReadOnlyList<DebugLine> lines, ICamera camera)
        {
            this._currentShader = null;

            cl.SetFramebuffer(this._renderTexture.Framebuffer);

            var viewProjectionMatrix = camera.ViewProjectionMatrix;
            cl.UpdateBuffer(this._viewProjectionBuffer, 0, ref viewProjectionMatrix);

            this._RenderLinesVertices(cl, lines);
        }

        private void _RenderLinesVertices(CommandList cl, IReadOnlyList<DebugLine> lines)
        {
            this._verticesCount = 0;

            for (var i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (this._verticesCount + 2 >= VERTICES_PER_BATCH)
                {
                    this._FlushVertices(cl, this._lineShader);
                }

                this._vertices[this._verticesCount++] = new Vertex { Position = line.Start, Color = line.Color.ToPackedUInt() };
                this._vertices[this._verticesCount++] = new Vertex { Position = line.End, Color = line.Color.ToPackedUInt() };
            }

            if (this._verticesCount > 0)
            {
                this._FlushVertices(cl, this._lineShader);
            }
        }

        private void _FlushVertices(CommandList cl, Shader shader)
        {
            cl.UpdateBuffer(this._vertexBuffer, 0, this._vertices);

            if (this._currentShader != shader)
            {
                this._currentShader = shader;
                var pipeline = shader.GetPipeline(this._vertexFormat);
                cl.SetPipeline(pipeline);
            }

            cl.SetVertexBuffer(0, this._vertexBuffer);
            cl.SetGraphicsResourceSet(0, this._passResourceSet);
            cl.Draw((uint)this._verticesCount);
            this._verticesCount = 0;
        }

        Pipeline IPipelineProvider.MakePipeline(ShaderVariant shaderVariant)
        {
            var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Wireframe,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: true
            );

            return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
            {
                DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual,
                PrimitiveTopology = PrimitiveTopology.LineList,
                ShaderSet = shaderVariant.ShaderSetDescription,
                BlendState = BlendStateDescription.SingleAlphaBlend,
                RasterizerState = rasterizerState,
                Outputs = this._renderTexture.OutputDescription,
                ResourceLayouts = new ResourceLayout[] {
                    this._passResourceLayout,
                },
            });
        }

        public void Dispose()
        {
            this._vertexBuffer.Dispose();
            this._viewProjectionBuffer.Dispose();
            this._passResourceLayout.Dispose();
            this._passResourceSet.Dispose();
            this._lineShader.Dispose();
        }
    }
}