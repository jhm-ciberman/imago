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
    public class SpriteBatcher : IFontStashRenderer, System.IDisposable
    {

        struct Vertex
        {
            public Vector3 position;
            public Vector2 uv;
            public uint color;

            public Vertex(Vector3 position, Vector2 uv, uint color)
            {
                this.position = position;
                this.uv = uv;
                this.color = color;
            }
        }

        private CommandList _commandList;
        private DeviceBuffer _cameraInfoBuffer;
        private ResourceFactory _factory;
        private GraphicsDevice _gd;
        private Pipeline _pipeline;
        private ResourceLayout _resourceLayout;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private Dictionary<GPUTexture, ResourceSet> _textureSets;
        
        private int _maxBatchSize = 1000;
        private GPUTexture? _batchTexture = null;
        private Vertex[] _batchVertices;
        private int _batchCount = 0;

        private int _totalDrawCalls = 0;
        private int _totalSpritesToDraw = 0;


        public SpriteBatcher(GraphicsDevice gd, CommandList commandList, OutputDescription outputDescription)
        {
            this._gd = gd;
            this._commandList = commandList;
            this._factory = gd.ResourceFactory;

            string vertexCode   = File.ReadAllText("res/shaders/sprites.vert");
            string fragmentCode = File.ReadAllText("res/shaders/sprites.frag");
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

            var indexBufferSize = sizeof(ushort) * 6 * this._maxBatchSize;
            var vertexBufferSize = Marshal.SizeOf<Vertex>() * 4 * this._maxBatchSize;
            this._indexBuffer = this._factory.CreateBuffer(new BufferDescription((uint) indexBufferSize, BufferUsage.IndexBuffer));
            this._vertexBuffer = this._factory.CreateBuffer(new BufferDescription((uint) vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            
            this._batchVertices = new Vertex[this._maxBatchSize * 4];
            ushort[] indices = new ushort[this._maxBatchSize * 6];

            ushort[] indicesTemplate = new ushort[] {};
            for (int i = 0; i < this._maxBatchSize; i++) {
                int j = i * 6;
                int offset = i * 4;
                indices[j + 0] = (ushort) (offset + 0);
                indices[j + 1] = (ushort) (offset + 1);
                indices[j + 2] = (ushort) (offset + 2);
                indices[j + 3] = (ushort) (offset + 0);
                indices[j + 4] = (ushort) (offset + 2);
                indices[j + 5] = (ushort) (offset + 3);
            }
            this._gd.UpdateBuffer(this._indexBuffer, 0, indices);

            this._textureSets = new Dictionary<GPUTexture, ResourceSet>();
        }

        public void BeginBatch(Matrix4x4 projectionMatrix)
        {
            this._gd.UpdateBuffer(this._cameraInfoBuffer, 0, projectionMatrix);
            this._totalDrawCalls = 0;
            this._totalSpritesToDraw = 0;
        }

        void IFontStashRenderer.Draw(
            ITexture2D texture, PointF position, Rectangle? sourceRectangle, Color color, 
            float rotation, PointF origin, PointF scale, float depth
        )
        {
            GPUTexture gpuTexture = (GPUTexture) texture;
            int color32 = (color.A << 24) + (color.B << 16) + (color.G << 8) + (color.R << 0);

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 size, uv, deltaUV;
            if (sourceRectangle == null) {
                size.X = gpuTexture.width;
                size.Y = gpuTexture.height;
                uv = Vector2.Zero;
                deltaUV = Vector2.One;
            } else {
                var r = sourceRectangle.Value;
                size.X = r.Width;
                size.Y = r.Height;
                uv.X = (float) r.X / (float) gpuTexture.width;
                uv.Y = (float) r.Y / (float) gpuTexture.height;
                deltaUV.X = size.X / (float) gpuTexture.width;
                deltaUV.Y = size.Y / (float) gpuTexture.height;
            }

            this.Draw(gpuTexture, pos, size, uv, deltaUV, (uint) color32, depth);
        }

        public void Draw(GPUTexture texture, Vector2 position, Vector2 size)
        {
            this.Draw(texture, position, size, Vector2.Zero, Vector2.One, 0xffffffff, 0f);
        }

        public void Draw(GPUTexture texture, Vector2 position, Vector2 size, Vector2 uv, Vector2 deltaUV, uint color32, float depth = 0f)
        {
            if (this._batchTexture != texture) {
                this.FlushCurrentBatch();
                this._batchTexture = texture;
            }

            float x = position.X;
            float y = position.Y;
            float w = size.X;
            float h = size.Y;
            float u = uv.X;
            float v = uv.Y;
            float du = deltaUV.X;
            float dv = deltaUV.Y;

            int i = this._batchCount * 4;
            this._batchVertices[i + 0] = new Vertex(new Vector3(x    , y    , depth), new Vector2(u     , v     ), color32);
            this._batchVertices[i + 1] = new Vertex(new Vector3(x + w, y    , depth), new Vector2(u + du, v     ), color32);
            this._batchVertices[i + 2] = new Vertex(new Vector3(x + w, y + h, depth), new Vector2(u + du, v + dv), color32);
            this._batchVertices[i + 3] = new Vertex(new Vector3(x    , y + h, depth), new Vector2(u     , v + dv), color32);
            this._batchCount++;
            this._totalSpritesToDraw++;

            if (this._batchCount >= this._maxBatchSize) {
                this.FlushCurrentBatch();
            }
        }

        public void EndBatch()
        {
            this.FlushCurrentBatch();
            //System.Console.WriteLine("DrawCalls: " + this._totalDrawCalls + ". Total sprites " + this._totalSpritesToDraw + ". Total textures: " + this._textureSets.Count);
            this._totalDrawCalls = 0;
            this._totalSpritesToDraw = 0;
        }

        private ResourceSet _GetResourceSetOrNew(GPUTexture texture)
        {
            if (! this._textureSets.TryGetValue(texture, out ResourceSet? resourceSet)) {
                System.Console.WriteLine("Create texture");
                resourceSet = this._factory.CreateResourceSet(
                    new ResourceSetDescription(this._resourceLayout, this._cameraInfoBuffer, texture.textureView, this._gd.LinearSampler)
                );
                this._textureSets.Add(texture, resourceSet);
            }
            return resourceSet;
        }

        public void FlushCurrentBatch()
        {
            if (this._batchTexture != null && this._batchCount > 0) {

                this._gd.UpdateBuffer(this._vertexBuffer, 0, this._batchVertices);

                this._commandList.SetPipeline(this._pipeline);
                this._commandList.SetGraphicsResourceSet(0, this._GetResourceSetOrNew(this._batchTexture));
                this._commandList.SetIndexBuffer(this._indexBuffer, IndexFormat.UInt16);
                this._commandList.SetVertexBuffer(0, this._vertexBuffer);
                this._commandList.DrawIndexed(
                    indexCount: (uint) this._batchCount * 6,
                    instanceCount: 1,
                    indexStart: 0,
                    vertexOffset: 0,
                    instanceStart: 0
                );
                this._totalDrawCalls++;
            }

            this._batchCount = 0;
            this._batchTexture = null;
        }

        public void Dispose()
        {
            this._pipeline.Dispose();
            this._resourceLayout.Dispose();
            this._cameraInfoBuffer.Dispose();
            this._indexBuffer.Dispose();
            this._vertexBuffer.Dispose();

            foreach (var set in this._textureSets.Values) {
                set.Dispose();
            }
        }
    }
}