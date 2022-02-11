using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using LifeSim.Engine.SceneGraph;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class GizmosPass : IDisposable, IRenderingPass
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
    private readonly Pipeline _pipeline;
    private readonly ResourceSet _passResourceSet;
    private readonly ResourceLayout _passResourceLayout;
    private readonly IRenderTexture _renderTexture;
    private readonly DeviceBuffer _vertexBuffer;

    private readonly GraphicsDevice _gd;

    private readonly DeviceBuffer _viewProjectionBuffer;

    private readonly VertexFormat _vertexFormat;

    public GizmosPass(Renderer renderer, IRenderTexture renderTexture)
    {
        this._renderTexture = renderTexture;
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

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

        var shaderVariant = new ShaderVariant(this._gd, this._vertexFormat, null, this._vertex, this._fragment);
        this._pipeline = this.MakePipeline(shaderVariant);
    }

    public void Render(CommandList cl, Scene scene)
    {
        Camera3D? camera = scene.Camera;
        if (camera == null || scene.Gizmos.Lines.Count == 0)
        {
            return;
        }

        cl.SetFramebuffer(this._renderTexture.Framebuffer);
        cl.SetPipeline(this._pipeline);

        var viewProjectionMatrix = camera.ViewProjectionMatrix;
        cl.UpdateBuffer(this._viewProjectionBuffer, 0, ref viewProjectionMatrix);

        this.RenderLinesVertices(cl, scene.Gizmos.Lines);
    }

    private void RenderLinesVertices(CommandList cl, IReadOnlyList<DebugLine> lines)
    {
        this._verticesCount = 0;

        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];

            if (this._verticesCount + 2 >= VERTICES_PER_BATCH)
            {
                this.FlushVertices(cl);
            }

            this._vertices[this._verticesCount++] = new Vertex { Position = line.Start, Color = line.Color.ToPackedUInt() };
            this._vertices[this._verticesCount++] = new Vertex { Position = line.End, Color = line.Color.ToPackedUInt() };
        }

        if (this._verticesCount > 0)
        {
            this.FlushVertices(cl);
        }
    }

    private void FlushVertices(CommandList cl)
    {
        cl.UpdateBuffer(this._vertexBuffer, 0, this._vertices);
        cl.SetVertexBuffer(0, this._vertexBuffer);
        cl.SetGraphicsResourceSet(0, this._passResourceSet);
        cl.Draw((uint)this._verticesCount);
        this._verticesCount = 0;
    }

    private Pipeline MakePipeline(ShaderVariant shaderVariant)
    {
        var rasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Wireframe,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
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
        this._pipeline.Dispose();
    }

    private readonly string _fragment = @"
            #version 450
            layout(location = 0) in vec4 fsin_Color;
            layout(location = 0) out vec4 fsout_color;

            void main()
            {
                fsout_color = fsin_Color;
            }
            ";

    private readonly string _vertex = @"
            #version 450
            layout(set = 0, binding = 0, std140) uniform CameraDataBuffer {
                mat4 ViewProjection;
            };

            layout(location = 0) in vec3 Position;
            layout(location = 1) in vec4 Color;

            layout(location = 0) out vec4 fsin_Color;

            void main()
            {
                gl_Position = ViewProjection * vec4(Position, 1);
                fsin_Color = Color;
            }
            ";
}