using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using FontStashSharp.Interfaces;
using Veldrid;
using Rectangle = System.Drawing.Rectangle;

namespace LifeSim.Rendering
{
    public class SpriteBatcher : IFontStashRenderer, System.IDisposable
    {
        class SpriteBatch : IRenderable
        {
            public VertexLayoutKind vertexLayoutKind => VertexLayoutKind.Sprite;

            public ResourceLayout? resourceLayout => null;

            public string[] GetShaderKeywords() => System.Array.Empty<string>();
        }

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
        private ResourceFactory _factory;
        private GraphicsDevice _gd;
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private Dictionary<GPUTexture, IMaterial> _materials = new Dictionary<GPUTexture, IMaterial>();
        
        private int _maxBatchSize = 1000;
        private GPUTexture? _batchTexture = null;
        private Vertex[] _batchVertices;
        private int _batchCount = 0;

        private int _totalDrawCalls = 0;
        private int _totalSpritesToDraw = 0;

        private PSOManager _psoManager;
        private GPURenderer _renderer;
        private SpriteBatch _renderable = new SpriteBatch();

        public SpriteBatcher(GraphicsDevice gd, PSOManager psoManager, GPURenderer renderer, CommandList commandList)
        {
            this._gd = gd;
            this._renderer = renderer;
            this._psoManager = psoManager;
            this._commandList = commandList;
            this._factory = gd.ResourceFactory;

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
                indices[j + 1] = (ushort) (offset + 2);
                indices[j + 2] = (ushort) (offset + 1);
                indices[j + 3] = (ushort) (offset + 0);
                indices[j + 4] = (ushort) (offset + 3);
                indices[j + 5] = (ushort) (offset + 2);
            }
            this._gd.UpdateBuffer(this._indexBuffer, 0, indices);
        }

        public void BeginBatch()
        {
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
            //System.Console.WriteLine("DrawCalls: " + this._totalDrawCalls + ". Total sprites " + this._totalSpritesToDraw + ". Total materials: " + this._materials.Count);
            this._totalDrawCalls = 0;
            this._totalSpritesToDraw = 0;
        }

        private IMaterial _GetMaterialOrNew(GPUTexture texture)
        {
            if (! this._materials.TryGetValue(texture, out IMaterial? material)) {
                System.Console.WriteLine("Create material");
                material = this._renderer.MakeSpritesMaterial(texture.deviceTexture);
                this._materials.Add(texture, material);
            }
            return material;
        }

        public void FlushCurrentBatch()
        {
            if (this._batchTexture != null && this._batchCount > 0) {

                this._commandList.UpdateBuffer(this._vertexBuffer, 0, this._batchVertices);

                var material = this._GetMaterialOrNew(this._batchTexture);
                var pipeline = this._psoManager.GetPipeline(material.pass, material, this._renderable);
                this._commandList.SetPipeline(pipeline);
                this._commandList.SetGraphicsResourceSet(0, material.pass.resourceSet);
                this._commandList.SetGraphicsResourceSet(1, material.resourceSet);
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
            this._indexBuffer.Dispose();
            this._vertexBuffer.Dispose();

            foreach (var set in this._materials.Values) {
                set.Dispose();
            }
        }
    }
}