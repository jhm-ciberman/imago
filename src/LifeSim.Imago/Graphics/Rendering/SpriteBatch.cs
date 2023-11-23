using System;
using System.Numerics;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Textures;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Graphics.Rendering;

public class SpriteBatch
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

        public Vertex(Vector3 position, Vector2 uv, uint color)
        {
            this.Position = position;
            this.Uv = uv;
            this.Color = color;
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

    public Shader? Shader { get; set; }

    public ITexture? Texture { get; set; }

    public Item[] Items { get; }

    public int Count { get; private set; }

    public SpriteBatch(int capacity)
    {
        this.Items = new Item[capacity];
    }

    public bool IsFull => this.Count >= this.Items.Length;

    public RenderFlags RenderFlags { get; internal set; } = RenderFlags.Transparent;

    public void Add(Item item)
    {
        if (this.IsFull)
            throw new InvalidOperationException("The sprite batch is full.");

        this.Items[this.Count++] = item;
    }

    public struct Item
    {
        public Vertex TopLeft { get; set; }
        public Vertex TopRight { get; set; }
        public Vertex BottomRight { get; set; }
        public Vertex BottomLeft { get; set; }
    }

    public float Opacity { get; set; }

    protected void UpdateColor(ref Color color)
    {
        color = new Color(color.R, color.G, color.B, (byte)(color.A * this.Opacity));
    }

    protected void UpdateColor(ref uint color)
    {
        color = color & 0x00FFFFFF | (uint)((color >> 24) * this.Opacity) << 24;
    }

    public void DrawCore(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color)
    {
        this.UpdateColor(ref color);

        this.Add(new Item
        {
            TopLeft = new Vertex(position.X, position.Y, 0f, uvTopLeft.X, uvTopLeft.Y, color),
            TopRight = new Vertex(position.X + size.X, position.Y, 0f, uvBottomRight.X, uvTopLeft.Y, color),
            BottomLeft = new Vertex(position.X, position.Y + size.Y, 0f, uvTopLeft.X, uvBottomRight.Y, color),
            BottomRight = new Vertex(position.X + size.X, position.Y + size.Y, 0f, uvBottomRight.X, uvBottomRight.Y, color),
        });
    }

    public void DrawCore(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color, Vector2 scale, float rotation, Vector2 origin, float depth = 0f)
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

        this.UpdateColor(ref color);

        this.Add(new Item
        {
            TopLeft = new Vertex(tlPos, tlUVs, color),
            TopRight = new Vertex(trPos, trUVs, color),
            BottomLeft = new Vertex(blPos, blUVs, color),
            BottomRight = new Vertex(brPos, brUVs, color),
        });
    }

    public void DrawCore(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, in Matrix3x2 transform, Color color, float depth = 0f)
    {
        var tl = position;
        var tr = position + new Vector2(size.X, 0f);
        var bl = position + new Vector2(0f, size.Y);
        var br = position + size;

        var tlUVs = uvTopLeft;
        var trUVs = new Vector2(uvBottomRight.X, uvTopLeft.Y);
        var blUVs = new Vector2(uvTopLeft.X, uvBottomRight.Y);
        var brUVs = uvBottomRight;

        this.UpdateColor(ref color);

        this.Add(new Item
        {
            TopLeft = new Vertex(Vector2.Transform(tl, transform), depth, tlUVs, color),
            TopRight = new Vertex(Vector2.Transform(tr, transform), depth, trUVs, color),
            BottomLeft = new Vertex(Vector2.Transform(bl, transform), depth, blUVs, color),
            BottomRight = new Vertex(Vector2.Transform(br, transform), depth, brUVs, color),
        });
    }

    public void DrawCore(ref Vertex topLeft, ref Vertex topRight, ref Vertex bottomLeft, ref Vertex bottomRight)
    {
        if (this.Opacity < 1f)
        {
            this.UpdateColor(ref topLeft.Color);
            this.UpdateColor(ref topRight.Color);
            this.UpdateColor(ref bottomLeft.Color);
            this.UpdateColor(ref bottomRight.Color);
        }

        this.Add(new Item
        {
            TopLeft = topLeft,
            TopRight = topRight,
            BottomLeft = bottomLeft,
            BottomRight = bottomRight,
        });
    }


    public void Clear()
    {
        this.Count = 0;
    }

    public void Reset()
    {
        this.Count = 0;
        this.Opacity = 1f;
        this.RenderFlags = RenderFlags.Transparent;
    }
}
