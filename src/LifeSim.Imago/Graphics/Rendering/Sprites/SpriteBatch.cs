using System;
using System.Numerics;
using LifeSim.Imago.Graphics.Materials;
using LifeSim.Imago.Graphics.Textures;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.Graphics.Rendering.Sprites;

internal class SpriteBatch
{
    public record struct Item(SpriteVertex TopLeft, SpriteVertex TopRight, SpriteVertex BottomRight, SpriteVertex BottomLeft);

    /// <summary>
    /// Gets or sets the shader used to render the sprites.
    /// </summary>
    public Shader? Shader { get; set; }

    /// <summary>
    /// Gets or sets the texture used to render the sprites.
    /// </summary>
    public ITexture? Texture { get; set; }

    /// <summary>
    /// Gets the items in the sprite batch. The number of items is determined by <see cref="Count"/>, not by the length of the array.
    /// </summary>
    public Item[] Items { get; }

    /// <summary>
    /// Gets the number of items in the sprite batch.
    /// </summary>
    public int Count { get; private set; } = 0;

    /// <summary>
    /// Gets or sets the opacity of the sprites in the batch.
    /// </summary>
    public float Opacity { get; set; } = 1f;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpriteBatch"/> class with the specified capacity.
    /// </summary>
    /// <param name="capacity">The maximum number of items that the sprite batch can hold.</param>
    public SpriteBatch(int capacity)
    {
        this.Items = new Item[capacity];
    }

    /// <summary>
    /// Gets whether the sprite batch is full.
    /// </summary>
    public bool IsFull => this.Count >= this.Items.Length;

    /// <summary>
    /// Gets or sets the render flags for the sprite batch.
    /// </summary>
    public RenderFlags RenderFlags { get; internal set; } = RenderFlags.Transparent;

    /// <summary>
    /// Adds an item to the sprite batch.
    /// </summary>
    /// <param name="item">The item to add.</param>
    public void Add(Item item)
    {
        if (this.IsFull)
            throw new InvalidOperationException("The sprite batch is full.");

        this.Items[this.Count++] = item;
    }

    /// <summary>
    /// Draws a sprite at the specified position with the specified size, UV coordinates, and color.
    /// </summary>
    /// <param name="position">The position of the sprite.</param>
    /// <param name="size">The size of the sprite.</param>
    /// <param name="uvTopLeft">The UV coordinates of the top-left corner of the sprite.</param>
    /// <param name="uvBottomRight">The UV coordinates of the bottom-right corner of the sprite.</param>
    /// <param name="color">The color of the sprite.</param>
    public void DrawCore(Vector2 position, Vector2 size, Vector2 uvTopLeft, Vector2 uvBottomRight, Color color)
    {
        this.ApplyColorAlpha(ref color);

        this.Add(new Item
        {
            TopLeft = new SpriteVertex(position.X, position.Y, 0f, uvTopLeft.X, uvTopLeft.Y, color),
            TopRight = new SpriteVertex(position.X + size.X, position.Y, 0f, uvBottomRight.X, uvTopLeft.Y, color),
            BottomLeft = new SpriteVertex(position.X, position.Y + size.Y, 0f, uvTopLeft.X, uvBottomRight.Y, color),
            BottomRight = new SpriteVertex(position.X + size.X, position.Y + size.Y, 0f, uvBottomRight.X, uvBottomRight.Y, color),
        });
    }

    /// <summary>
    /// Draws a sprite at the specified position with the specified size, UV coordinates, color, scale, rotation, origin, and depth.
    /// </summary>
    /// <param name="position">The position of the sprite.</param>
    /// <param name="size">The size of the sprite.</param>
    /// <param name="uvTopLeft">The UV coordinates of the top-left corner of the sprite.</param>
    /// <param name="uvBottomRight">The UV coordinates of the bottom-right corner of the sprite.</param>
    /// <param name="color">The color of the sprite.</param>
    /// <param name="scale">The scale of the sprite.</param>
    /// <param name="rotation">The rotation of the sprite.</param>
    /// <param name="origin">The origin of the sprite.</param>
    /// <param name="depth">The depth of the sprite.</param>
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

        this.ApplyColorAlpha(ref color);

        this.Add(new Item
        {
            TopLeft = new SpriteVertex(tlPos, tlUVs, color),
            TopRight = new SpriteVertex(trPos, trUVs, color),
            BottomLeft = new SpriteVertex(blPos, blUVs, color),
            BottomRight = new SpriteVertex(brPos, brUVs, color),
        });
    }

    /// <summary>
    /// Draws a sprite at the specified position with the specified size, UV coordinates, transform, color, and depth.
    /// </summary>
    /// <param name="position">The position of the sprite.</param>
    /// <param name="size">The size of the sprite.</param>
    /// <param name="uvTopLeft">The UV coordinates of the top-left corner of the sprite.</param>
    /// <param name="uvBottomRight">The UV coordinates of the bottom-right corner of the sprite.</param>
    /// <param name="transform">A 2D transformation matrix that will be applied to the sprite.</param>
    /// <param name="color">The color of the sprite.</param>
    /// <param name="depth">The depth of the sprite.</param>
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

        this.ApplyColorAlpha(ref color);

        this.Add(new Item
        {
            TopLeft = new SpriteVertex(Vector2.Transform(tl, transform), depth, tlUVs, color),
            TopRight = new SpriteVertex(Vector2.Transform(tr, transform), depth, trUVs, color),
            BottomLeft = new SpriteVertex(Vector2.Transform(bl, transform), depth, blUVs, color),
            BottomRight = new SpriteVertex(Vector2.Transform(br, transform), depth, brUVs, color),
        });
    }

    /// <summary>
    /// Draws a sprite at the specified position with the specified size, UV coordinates, transform, color, and depth.
    /// </summary>
    /// <param name="topLeft">The top-left vertex of the sprite.</param>
    /// <param name="topRight">The top-right vertex of the sprite.</param>
    /// <param name="bottomLeft">The bottom-left vertex of the sprite.</param>
    /// <param name="bottomRight">The bottom-right vertex of the sprite.</param>
    public void DrawCore(ref SpriteVertex topLeft, ref SpriteVertex topRight, ref SpriteVertex bottomLeft, ref SpriteVertex bottomRight)
    {
        if (this.Opacity < 1f)
        {
            this.ApplyColorAlpha(ref topLeft.Color);
            this.ApplyColorAlpha(ref topRight.Color);
            this.ApplyColorAlpha(ref bottomLeft.Color);
            this.ApplyColorAlpha(ref bottomRight.Color);
        }

        this.Add(new Item
        {
            TopLeft = topLeft,
            TopRight = topRight,
            BottomLeft = bottomLeft,
            BottomRight = bottomRight,
        });
    }

    /// <summary>
    /// Clears all items from the sprite batch.
    /// </summary>
    public void Clear()
    {
        this.Count = 0;
    }

    /// <summary>
    /// Resets the sprite batch to its initial state.
    /// </summary>
    public void Reset()
    {
        this.Count = 0;
        this.Opacity = 1f;
        this.RenderFlags = RenderFlags.Transparent;
    }

    protected void ApplyColorAlpha(ref Color color)
    {
        color = new Color(color.R, color.G, color.B, (byte)(color.A * this.Opacity));
    }

    protected void ApplyColorAlpha(ref uint color)
    {
        color = color & 0x00FFFFFF | (uint)((color >> 24) * this.Opacity) << 24;
    }
}
