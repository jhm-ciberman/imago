using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Imago.Graphics3D.SceneGraph;
using Imago.Graphics3D.SceneGraph.Cameras;
using Imago.Rendering;
using Imago.Textures;
using NeoVeldrid;

namespace Imago.Graphics3D.Rendering.Passes;

internal class GizmosPass : IDisposable
{
    private const int VerticesPerBatch = 1000;

    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        public Vector3 Position;
        public uint Color;
    }

    private readonly Vertex[] _vertices = new Vertex[VerticesPerBatch];

    private readonly Pipeline _depthTestedPipeline;

    private readonly Pipeline _inFrontPipeline;

    private readonly ResourceSet _passResourceSet;

    private readonly ResourceLayout _passResourceLayout;

    private readonly DeviceBuffer _vertexBuffer;

    private readonly GraphicsDevice _gd;

    private readonly DeviceBuffer _viewProjectionBuffer;

    private int _verticesCount = 0;

    public GizmosPass(Renderer renderer)
    {
        this._gd = renderer.GraphicsDevice;
        var factory = this._gd.ResourceFactory;

        this._vertexBuffer = factory.CreateBuffer(new BufferDescription((uint)(VerticesPerBatch * Marshal.SizeOf<Vertex>()), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        this._viewProjectionBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

        this._passResourceLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
            new ResourceLayoutElementDescription("CameraDataBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex)
        ));

        this._passResourceSet = factory.CreateResourceSet(new ResourceSetDescription(this._passResourceLayout, this._viewProjectionBuffer));

        var vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
        );

        var shaders = ShaderCompiler.CompileShaders(this._gd, this._vertex, this._fragment);
        var outputs = renderer.MainRenderTexture.OutputDescription;
        this._depthTestedPipeline = this.CreatePipeline(vertexLayout, shaders, outputs, DepthStencilStateDescription.DepthOnlyLessEqual);
        this._inFrontPipeline = this.CreatePipeline(vertexLayout, shaders, outputs, DepthStencilStateDescription.Disabled);
    }

    /// <summary>
    /// Render the debug gizmos to the render texture.
    /// </summary>
    /// <param name="cl">The command list to use for rendering.</param>
    /// <param name="renderTexture">The render texture to render the gizmos to.</param>
    /// <param name="camera">The camera to use for rendering.</param>
    /// <param name="gizmos">The gizmos to render.</param>
    public void Render(CommandList cl, RenderTexture renderTexture, Camera camera, GizmosDrawer gizmos)
    {
        if (gizmos.Lines.Count == 0) return;

        cl.SetFramebuffer(renderTexture.Framebuffer);

        var viewProjectionMatrix = camera.ViewProjectionMatrix;
        cl.UpdateBuffer(this._viewProjectionBuffer, 0, ref viewProjectionMatrix);

        cl.SetPipeline(this._depthTestedPipeline);
        this.RenderLinesVertices(cl, gizmos.Lines, drawInFront: false);

        cl.SetPipeline(this._inFrontPipeline);
        this.RenderLinesVertices(cl, gizmos.Lines, drawInFront: true);
    }

    private Pipeline CreatePipeline(VertexLayoutDescription vertexLayout, Shader[] shaders, OutputDescription outputs, DepthStencilStateDescription depthStencilState)
    {
        return this._gd.ResourceFactory.CreateGraphicsPipeline(new GraphicsPipelineDescription()
        {
            DepthStencilState = depthStencilState,
            PrimitiveTopology = PrimitiveTopology.LineList,
            ShaderSet = new ShaderSetDescription([vertexLayout], shaders),
            BlendState = BlendStateDescription.SingleAlphaBlend,
            RasterizerState = new RasterizerStateDescription(
                FaceCullMode.None,
                PolygonFillMode.Wireframe,
                FrontFace.Clockwise,
                depthClipEnabled: true,
                scissorTestEnabled: false
            ),
            Outputs = outputs,
            ResourceLayouts = [this._passResourceLayout],
        });
    }

    private void RenderLinesVertices(CommandList cl, IReadOnlyList<DebugLine> lines, bool drawInFront)
    {
        this._verticesCount = 0;

        for (var i = 0; i < lines.Count; i++)
        {
            DebugLine line = lines[i];
            if (line.DrawInFront != drawInFront) continue;

            if (this._verticesCount + 2 >= VerticesPerBatch)
                this.FlushVertices(cl);

            this._vertices[this._verticesCount++] = new Vertex { Position = line.Start, Color = line.Color.ToPackedUInt() };
            this._vertices[this._verticesCount++] = new Vertex { Position = line.End, Color = line.Color.ToPackedUInt() };
        }

        if (this._verticesCount > 0)
            this.FlushVertices(cl);
    }

    private void FlushVertices(CommandList cl)
    {
        cl.UpdateBuffer(this._vertexBuffer, 0, this._vertices);
        cl.SetVertexBuffer(0, this._vertexBuffer);
        cl.SetGraphicsResourceSet(0, this._passResourceSet);
        cl.Draw((uint)this._verticesCount);
        this._verticesCount = 0;
    }

    public void Dispose()
    {
        this._vertexBuffer.Dispose();
        this._viewProjectionBuffer.Dispose();
        this._passResourceLayout.Dispose();
        this._passResourceSet.Dispose();
        this._depthTestedPipeline.Dispose();
        this._inFrontPipeline.Dispose();
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
