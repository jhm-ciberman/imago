using System.Numerics;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Rendering.Sprites;

public struct SpriteVertex
{
    public Vector3 Position;
    public Vector2 Uv;
    public uint Color;

    public SpriteVertex(Vector3 position, Vector2 uv, Color color)
    {
        this.Position = position;
        this.Uv = uv;
        this.Color = color.ToPackedUInt();
    }

    public SpriteVertex(Vector3 position, Vector2 uv, uint color)
    {
        this.Position = position;
        this.Uv = uv;
        this.Color = color;
    }

    public SpriteVertex(Vector2 position, float depth, Vector2 uv, Color color)
    {
        this.Position = new Vector3(position, depth);
        this.Uv = uv;
        this.Color = color.ToPackedUInt();
    }

    public SpriteVertex(float x, float y, float z, float u, float v, Color color)
    {
        this.Position = new Vector3(x, y, z);
        this.Uv = new Vector2(u, v);
        this.Color = color.ToPackedUInt();
    }
}
