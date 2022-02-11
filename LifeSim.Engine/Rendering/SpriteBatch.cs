using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace LifeSim.Engine.Rendering;

public class SpriteBatch : IDisposable
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 Uv;
        public uint Color;

        public Vertex(Vector3 position, Vector2 uv, Color color)
        {
            this.Position = position;
            this.Uv = uv;
            this.Color = color.ToPackedUInt();
        }

        public Vertex(Vector2 position, float depth, Vector2 uv, Color color)
        {
            this.Position = new Vector3(position, depth);
            this.Uv = uv;
            this.Color = color.ToPackedUInt();
        }

        public Vertex(float x, float y, float z, float u, float v, Color color)
        {
            this.Position = new Vector3(x, y, z);
            this.Uv = new Vector2(u, v);
            this.Color = color.ToPackedUInt();
        }
    }

    public struct Item
    {
        public Vertex TopLeft;
        public Vertex TopRight;
        public Vertex BottomRight;
        public Vertex BottomLeft;
    }

    public ITexture Texture { get; private set; }
    public Shader Shader { get; private set; }

    public DeviceBuffer VertexBuffer { get; private set; }

    public int Count { get; private set; } = 0;

    private readonly int _capacity = 1000;
    public Item[] Items { get; }

    public ResourceSet ResourceSet { get; private set; }
    private readonly GraphicsDevice _gd;

    public SpriteBatch(GraphicsDevice gd, Shader shader, ITexture texture, int batchCapacity)
    {
        this._gd = gd;
        this.Shader = shader;
        this.Texture = texture;
        var factory = this._gd.ResourceFactory;
        this.ResourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
            shader.MaterialResourceLayout, this.Texture.DeviceTexture, this.Texture.Sampler));
        this._capacity = batchCapacity;
        this.Items = new Item[batchCapacity * 4];

        var vertexBufferSize = (uint) (Marshal.SizeOf<Item>() * 4 * batchCapacity);
        this.VertexBuffer = factory.CreateBuffer(new BufferDescription((uint)vertexBufferSize, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
    }

    public bool IsFull => this.Count >= this._capacity;

    public void Draw(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color, float depth = 0f)
    {
        float w = size.X;
        float h = size.Y;

        this.Items[this.Count++] = new Item
        {
            TopLeft = new Vertex(position.X, position.Y, depth, uvTopLeft.X, uvTopLeft.Y, color),
            TopRight = new Vertex(position.X + w, position.Y, depth, uvBottomRight.X, uvTopLeft.Y, color),
            BottomLeft = new Vertex(position.X, position.Y + h, depth, uvTopLeft.X, uvBottomRight.Y, color),
            BottomRight = new Vertex(position.X + w, position.Y + h, depth, uvBottomRight.X, uvBottomRight.Y, color),
        };
    }

    public void Draw(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color, Vector2 scale, float rotation, Vector2 origin, float depth = 0f)
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

        var tlUVs = uvTopLeft;
        var trUVs = new Vector2(uvBottomRight.X, uvTopLeft.Y);
        var blUVs = new Vector2(uvTopLeft.X, uvBottomRight.Y);
        var brUVs = uvBottomRight;

        this.Items[this.Count++] = new Item
        {
            TopLeft = new Vertex(tlPos, tlUVs, color),
            TopRight = new Vertex(trPos, trUVs, color),
            BottomLeft = new Vertex(blPos, blUVs, color),
            BottomRight = new Vertex(brPos, brUVs, color),
        };
    }

    public void Draw(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color, float depth = 0f)
    {
        var tl = position;
        var tr = position + new Vector2(size.X, 0f);
        var bl = position + new Vector2(0f, size.Y);
        var br = position + size;

        var tlUVs = uvTopLeft;
        var trUVs = new Vector2(uvBottomRight.X, uvTopLeft.Y);
        var blUVs = new Vector2(uvTopLeft.X, uvBottomRight.Y);
        var brUVs = uvBottomRight;

        this.Items[this.Count++] = new Item
        {
            TopLeft = new Vertex(Vector2.Transform(tl, transform), depth, tlUVs, color),
            TopRight = new Vertex(Vector2.Transform(tr, transform), depth, trUVs, color),
            BottomLeft = new Vertex(Vector2.Transform(bl, transform), depth, blUVs, color),
            BottomRight = new Vertex(Vector2.Transform(br, transform), depth, brUVs, color),
        };
    }

    public void Draw(Vector2 position, System.Drawing.Rectangle? source, Color color, float rotation, Vector2 origin, Vector2 scale, float depth)
    {
        Vector2 pos = new Vector2(position.X, position.Y);
        Vector2 textureSize = new Vector2(this.Texture.Width, this.Texture.Height);
        Vector2 size, uvTopLeft, uvBottomRight;
        if (source == null)
        {
            size = textureSize;
            uvTopLeft = Vector2.Zero;
            uvBottomRight = Vector2.One;
        }
        else
        {
            var r = source.Value;
            size = new Vector2(r.Width, r.Height);
            uvTopLeft = new Vector2(r.X, r.Y) / textureSize;
            uvBottomRight = uvTopLeft + size / textureSize;
        }

        this.Draw(pos, size, uvTopLeft, uvBottomRight, color, scale, rotation, origin, depth);
    }

    public void SetMaterial(Shader shader, ITexture texture)
    {
        this.ResourceSet.Dispose(); // TODO: add to DisposeCollector
        this.Shader = shader;
        this.Texture = texture;
        this.ResourceSet = this._gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(
            shader.MaterialResourceLayout, this.Texture.DeviceTexture, this.Texture.Sampler));
    }

    public void Clear()
    {
        this.Count = 0;
    }

    public void Dispose()
    {
        this.VertexBuffer.Dispose();
    }
}