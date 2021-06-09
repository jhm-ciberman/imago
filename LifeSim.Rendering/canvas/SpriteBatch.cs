using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Rendering
{
    public class SpriteBatch : IDisposable
    {
        public struct Vertex
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

        public Texture texture { get; private set; }
        public Shader shader { get; private set; }

        public Veldrid.DeviceBuffer vertexBuffer { get; private set; }

        public int count { get; private set; } = 0;

        private readonly int capacity = 1000;
        public readonly Vertex[] vertices;

        public Veldrid.ResourceSet resourceSet;
        private Veldrid.GraphicsDevice _gd;

        public SpriteBatch(Veldrid.GraphicsDevice gd, Shader shader, Texture texture, int batchCapacity)
        {
            this._gd = gd;
            this.shader = shader;
            this.texture = texture;
            var factory = this._gd.ResourceFactory;
            this.resourceSet = shader.CreateResourceSet(this.texture.deviceTexture, this.texture.sampler);
            this.capacity = batchCapacity;
            this.vertices = new Vertex[batchCapacity * 4];

            var vertexBufferSize = (uint) (Marshal.SizeOf<SpriteBatch.Vertex>() * 4 * batchCapacity);
            this.vertexBuffer = factory.CreateBuffer(new BufferDescription((uint) vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        }

        public bool isFull => (this.count >= this.capacity);

        public void Draw(Vector2 position, Vector2 size, Vector2 uv, Vector2 deltaUV, uint color32, float depth = 0f)
        {
            float x = position.X;
            float y = position.Y;
            float w = size.X;
            float h = size.Y;
            float u = uv.X;
            float v = uv.Y;
            float du = deltaUV.X;
            float dv = deltaUV.Y;

            int i = this.count * 4;
            this.vertices[i + 0] = new Vertex(new Vector3(x    , y    , depth), new Vector2(u     , v     ), color32);
            this.vertices[i + 1] = new Vertex(new Vector3(x + w, y    , depth), new Vector2(u + du, v     ), color32);
            this.vertices[i + 2] = new Vertex(new Vector3(x + w, y + h, depth), new Vector2(u + du, v + dv), color32);
            this.vertices[i + 3] = new Vertex(new Vector3(x    , y + h, depth), new Vector2(u     , v + dv), color32);
            this.count++;
        }

        public void Draw(
            System.Drawing.PointF position, 
            System.Drawing.Rectangle? sourceRectangle, 
            System.Drawing.Color color, 
            float rotation, 
            System.Drawing.PointF origin, 
            System.Drawing.PointF scale, 
            float depth
        )
        {
            int color32 = (color.A << 24) + (color.B << 16) + (color.G << 8) + (color.R << 0);

            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 size, uv, deltaUV;
            if (sourceRectangle == null) {
                size.X = this.texture.width;
                size.Y = this.texture.height;
                uv = Vector2.Zero;
                deltaUV = Vector2.One;
            } else {
                var r = sourceRectangle.Value;
                size.X = r.Width;
                size.Y = r.Height;
                uv.X = (float) r.X / (float) this.texture.width;
                uv.Y = (float) r.Y / (float) this.texture.height;
                deltaUV.X = size.X / (float) this.texture.width;
                deltaUV.Y = size.Y / (float) this.texture.height;
            }

            this.Draw(pos, size, uv, deltaUV, (uint) color32, depth);
        }

        public void SetMaterial(Shader shader, Texture texture)
        {
            this.resourceSet.Dispose();
            this.shader = shader;
            this.texture = texture;
            this.resourceSet = shader.CreateResourceSet(this.texture.deviceTexture, this.texture.sampler);
        }

        public void Clear()
        {
            this.count = 0;
        }

        public void Dispose()
        {
            this.vertexBuffer.Dispose();
        }
    }
}