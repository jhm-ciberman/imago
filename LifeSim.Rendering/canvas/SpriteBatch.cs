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

            public Vertex(Vector3 position, Vector2 uv, Color color)
            {
                this.position = position;
                this.uv = uv;
                this.color = color.ToPackedUInt();
            }

            public Vertex(Vector2 position, float depth, Vector2 uv, Color color)
            {
                this.position = new Vector3(position, depth);
                this.uv = uv;
                this.color = color.ToPackedUInt();
            }

            public Vertex(float x, float y, float z, float u, float v, Color color)
            {
                this.position = new Vector3(x, y, z);
                this.uv = new Vector2(u, v);
                this.color = color.ToPackedUInt();
            }
        }

        public struct Item
        {
            public Vertex tl;
            public Vertex tr;
            public Vertex br;
            public Vertex bl;
        }

        public Texture texture { get; private set; }
        public Shader shader { get; private set; }

        public Veldrid.DeviceBuffer vertexBuffer { get; private set; }

        public int count { get; private set; } = 0;

        private readonly int capacity = 1000;
        public readonly Item[] items;

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
            this.items = new Item[batchCapacity * 4];

            var vertexBufferSize = (uint) (Marshal.SizeOf<SpriteBatch.Item>() * 4 * batchCapacity);
            this.vertexBuffer = factory.CreateBuffer(new BufferDescription((uint) vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
        }

        public bool isFull => (this.count >= this.capacity);

        public void Draw(Vector2 position, Vector2 size, Vector2 uv, Vector2 deltaUV, Color color, float depth = 0f)
        {
            float w = size.X;
            float h = size.Y;
            float du = deltaUV.X;
            float dv = deltaUV.Y;

            int i = this.count;
            this.items[i].tl = new Vertex(position.X    , position.Y    , depth, uv.X     , uv.Y     , color);
            this.items[i].tr = new Vertex(position.X + w, position.Y    , depth, uv.X + du, uv.Y     , color);
            this.items[i].br = new Vertex(position.X + w, position.Y + h, depth, uv.X + du, uv.Y + dv, color);
            this.items[i].bl = new Vertex(position.X    , position.Y + h, depth, uv.X     , uv.Y + dv, color);
            this.count++;
        }

        public void Draw(Vector2 position, Vector2 size, Vector2 uv, Vector2 deltaUV, Color color, Vector2 scale, float rotation, Vector2 origin, float depth = 0f)
        {
            // Adapted from https://github.com/ThomasMiz/TrippyGL/blob/109eaf483d3289c0214963b7d22bdbd320d243ed/TrippyGL/TextureBatchItem.cs#L90
            // Thank you! :D 
            float sin = MathF.Sin(rotation);
            float cos = MathF.Cos(rotation);

            var tl = -origin * scale;
            var tr = new Vector2(tl.X + size.X * scale.X, tl.Y);
            var bl = new Vector2(tl.X, tl.Y + size.Y * scale.Y);
            var br = new Vector2(tr.X, bl.Y);

            var tlPos = new Vector3(cos * tl.X - sin * tl.Y + position.X, sin * tl.X + cos * tl.Y + position.Y, depth);
            var trPos = new Vector3(cos * tr.X - sin * tr.Y + position.X, sin * tr.X + cos * tr.Y + position.Y, depth);
            var blPos = new Vector3(cos * bl.X - sin * bl.Y + position.X, sin * bl.X + cos * bl.Y + position.Y, depth);
            var brPos = new Vector3(cos * br.X - sin * br.Y + position.X, sin * br.X + cos * br.Y + position.Y, depth);

            var tlUVs = new Vector2(uv.X            , uv.Y            );
            var trUVs = new Vector2(uv.X + deltaUV.X, uv.Y            );
            var blUVs = new Vector2(uv.X            , uv.Y + deltaUV.Y);
            var brUVs = new Vector2(uv.X + deltaUV.X, uv.Y + deltaUV.Y);

            int i = this.count;
            this.items[i].tl = new Vertex(tlPos, tlUVs, color);
            this.items[i].tr = new Vertex(trPos, trUVs, color);
            this.items[i].bl = new Vertex(blPos, blUVs, color);
            this.items[i].br = new Vertex(brPos, brUVs, color);
            this.count++;
        }

        public void Draw(Vector2 position, Vector2 size, Vector2 uv, Vector2 deltaUV, in Matrix3x2 transform, Color color, float depth = 0f)
        {
            var tl = position;
            var tr = position + new Vector2(size.X, 0f);
            var bl = position + new Vector2(0f, size.Y);
            var br = position + size;

            var tlUVs = uv;
            var trUVs = uv + new Vector2(deltaUV.X, 0f);
            var blUVs = uv + new Vector2(0f, deltaUV.Y);
            var brUVs = uv + deltaUV;

            int i = this.count;
            this.items[i].tl = new Vertex(Vector2.Transform(tl, transform), depth, tlUVs, color);
            this.items[i].tr = new Vertex(Vector2.Transform(tr, transform), depth, trUVs, color);
            this.items[i].bl = new Vertex(Vector2.Transform(bl, transform), depth, blUVs, color);
            this.items[i].br = new Vertex(Vector2.Transform(br, transform), depth, brUVs, color);
            this.count++;
        }

        public void Draw(Vector2 position, System.Drawing.Rectangle? source, Color color, float rotation, Vector2 origin, Vector2 scale, float depth)
        {
            Vector2 pos = new Vector2(position.X, position.Y);
            Vector2 textureSize = new Vector2(this.texture.width, this.texture.height);
            Vector2 size, uv, deltaUV;
            if (source == null) {
                size = textureSize;
                uv = Vector2.Zero;
                deltaUV = Vector2.One;
            } else {
                var r = source.Value;
                size = new Vector2(r.Width, r.Height);
                uv = new Vector2(r.X, r.Y) / textureSize;
                deltaUV = size / textureSize;
            }

            this.Draw(pos, size, uv, deltaUV, color, scale, rotation, origin, depth);
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