using System;
using System.Numerics;

namespace LifeSim.Engine.Rendering;

public class SpriteBatch
{


    public Shader? Shader { get; set; }

    public ITexture? Texture { get; set; }

    public Item[] Items { get; }

    public int Count { get; private set; }

    public SpriteBatch(int capacity)
    {
        this.Items = new Item[capacity];
    }

    public bool IsFull => this.Count >= this.Items.Length;

    public void Add(Item item)
    {
        if (this.IsFull)
        {
            throw new InvalidOperationException("The sprite batch is full.");
        }

        this.Items[this.Count++] = item;
    }

    public struct Item
    {
        public Vertex TopLeft { get; set; }
        public Vertex TopRight { get; set; }
        public Vertex BottomRight { get; set; }
        public Vertex BottomLeft { get; set; }
    }


    public struct Vertex
    {
        public Vector3 Position { get; }
        public Vector2 Uv { get; }
        public uint Color { get; }

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

    public void Clear()
    {
        this.Count = 0;
    }
}