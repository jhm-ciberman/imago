using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using FontStashSharp.Interfaces;
using Veldrid;
using Veldrid.SPIRV;
using Rectangle = System.Drawing.Rectangle;

namespace LifeSim.Rendering
{
    public class TextBatcher : IFontStashRenderer, System.IDisposable
    {

        struct Vertex
        {
            public Vector3 position;
            public Vector2 uv;
            public int color;

            public Vertex(Vector3 position, Vector2 uv, int color)
            {
                this.position = position;
                this.uv = uv;
                this.color = color;
            }
        }

        struct Batch
        {
            public GPUTexture texture;
            public List<Vertex> vertices;
            public List<ushort> indices;
        }

        private CommandList _commandList;
        private DeviceBuffer _cameraInfoBuffer;
        private ResourceFactory _factory;
        private GraphicsDevice _gd;
        private Dictionary<GPUTexture, Batch> _batches = new Dictionary<GPUTexture, Batch>(); 
        private Pipeline _pipeline;
        private ResourceLayout _resourceLayout;

        public TextBatcher(GraphicsDevice gd, CommandList commandList, OutputDescription outputDescription)
        {
            this._gd = gd;
            this._commandList = commandList;
            this._factory = gd.ResourceFactory;

            string vertexCode   = File.ReadAllText("res/shaders/gui.vert");
            string fragmentCode = File.ReadAllText("res/shaders/gui.frag");
            var vertBytes = Encoding.UTF8.GetBytes(vertexCode);
            var fragBytes = Encoding.UTF8.GetBytes(fragmentCode);
            ShaderDescription vertexShaderDesc = new ShaderDescription(ShaderStages.Vertex, vertBytes, "main");
            ShaderDescription fragmentShaderDesc = new ShaderDescription(ShaderStages.Fragment, fragBytes, "main");
            var shaders = this._factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);
            
            this._cameraInfoBuffer = this._factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));

            this._resourceLayout = this._factory.CreateResourceLayout(new ResourceLayoutDescription(new [] {
                new ResourceLayoutElementDescription("CameraInfo", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("Texture", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment),
            }));

            var vertexLayout = new VertexLayoutDescription(
                new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                new VertexElementDescription("TextureCoords", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
            );
            
            GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
            pipelineDescription.ShaderSet = new ShaderSetDescription(new [] { vertexLayout }, shaders);
            pipelineDescription.BlendState = BlendStateDescription.SingleAlphaBlend;
            pipelineDescription.DepthStencilState = DepthStencilStateDescription.DepthOnlyLessEqual;
            pipelineDescription.RasterizerState = RasterizerStateDescription.CullNone; // TODO: change
            pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleList;
            pipelineDescription.ResourceLayouts = new ResourceLayout[] { this._resourceLayout };
            pipelineDescription.Outputs = outputDescription;

            this._pipeline = this._factory.CreateGraphicsPipeline(pipelineDescription);
        }

        public void BeginBatch(Matrix4x4 projectionMatrix)
        {
            this._batches.Clear();
            this._gd.UpdateBuffer(this._cameraInfoBuffer, 0, projectionMatrix);
        }

        void IFontStashRenderer.Draw(
            ITexture2D texture, PointF position, Rectangle? sourceRectangle, Color color, 
            float rotation, PointF origin, PointF scale, float depth
        )
        {
            GPUTexture gpuTexture = (GPUTexture) texture;

            Batch batch;
            if (! this._batches.TryGetValue(gpuTexture, out batch)) {
                batch = new Batch();
                batch.texture = gpuTexture;
                batch.vertices = new List<Vertex>(6 * 8);
                batch.indices = new List<ushort>(6 * 2);
                this._batches[gpuTexture] = batch;
            }

            float x = position.X;
            float y = position.Y;

            float w, h, u, v, du, dv;
            if (sourceRectangle == null) {
                w = gpuTexture.width;
                h = gpuTexture.height;
                u = 0f;
                v = 0f;
                du = 1f;
                dv = 1f;
            } else {
                var r = sourceRectangle.Value;
                w = r.Width;
                h = r.Height;
                u = (float) r.X / (float) gpuTexture.width;
                v = (float) r.Y / (float) gpuTexture.height;
                du = w / (float) gpuTexture.width;
                dv = h / (float) gpuTexture.height;
            }

            //System.Console.WriteLine("x:" + x + " y:" + y + " u:" + u + " v:" + v + "du:" + du + " dv:" + dv);
            int i = batch.vertices.Count;
            batch.indices.AddRange(new ushort[] { 
                (ushort) (i + 0), (ushort) (i + 1), (ushort) (i + 2), 
                (ushort) (i + 0), (ushort) (i + 2), (ushort) (i + 3), 
            });

            int color32 = (color.A << 24) + (color.B << 16) + (color.G << 8) + (color.R << 0);
            batch.vertices.AddRange(new [] {
                new Vertex(new Vector3(x    , y    , depth), new Vector2(u     , v     ), color32),
                new Vertex(new Vector3(x + w, y    , depth), new Vector2(u + du, v     ), color32),
                new Vertex(new Vector3(x + w, y + h, depth), new Vector2(u + du, v + dv), color32),
                new Vertex(new Vector3(x    , y + h, depth), new Vector2(u     , v + dv), color32),
            });
        }

        public void EndBatch()
        {
            foreach (var batch in this._batches.Values)
            {
                //System.Console.WriteLine(batch.vertices.Count / 4);
                var indexBufferSize = sizeof(ushort) * batch.indices.Count;
                var vertexBufferSize = Marshal.SizeOf<Vertex>() * batch.vertices.Count;
                var indexBuffer  = this._factory.CreateBuffer(new BufferDescription((uint) indexBufferSize, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
                var vertexBuffer = this._factory.CreateBuffer(new BufferDescription((uint) vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
                this._gd.UpdateBuffer(vertexBuffer, 0, batch.vertices.ToArray());
                this._gd.UpdateBuffer(indexBuffer, 0, batch.indices.ToArray());
                var resourceSet = this._factory.CreateResourceSet(
                    new ResourceSetDescription(this._resourceLayout, this._cameraInfoBuffer, batch.texture.textureView, this._gd.LinearSampler)
                );

                this._commandList.SetPipeline(this._pipeline);
                this._commandList.SetGraphicsResourceSet(0, resourceSet);
                this._commandList.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);
                this._commandList.SetVertexBuffer(0, vertexBuffer);
                this._commandList.DrawIndexed(
                    indexCount: (uint) batch.indices.Count,
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0
                );

                // TODO: created and disposed each frame? Ouch! 
                resourceSet.Dispose();
                indexBuffer.Dispose();
                vertexBuffer.Dispose();
            }
        }

        public void Dispose()
        {
            this._pipeline.Dispose();
            this._resourceLayout.Dispose();
            this._cameraInfoBuffer.Dispose();
        }
    }
}