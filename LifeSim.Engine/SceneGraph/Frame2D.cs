using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Support;

namespace LifeSim.Engine.SceneGraph;

public class Frame2D : RenderNode2D, ICanvasItem
{
    public ITexture? Texture { get; set; }

    public Shader? Shader { get; set; }

    public Vector2 Size { get; set; }

    private Vector2 _pivot = Vector2.Zero;
    public Vector2 Pivot
    {
        get => this._pivot;
        set { this._pivot = value; this.OnTransformDirty(); }
    }

    public Frame2D() { }
    public Frame2D(ITexture texture) : this(texture, new Vector2(texture.Width, texture.Height)) { }

    public Frame2D(ITexture texture, Vector2 size)
    {
        this.Texture = texture;
        this.Size = size;
    }

    public override void Render(SpriteBatcher spriteBatcher)
    {
        if (this.Texture != null)
        {
            spriteBatcher.DrawTexture(this.Shader, this.Texture, -this.Pivot, this.Size, Vector2.Zero, Vector2.One, in this.WorldMatrix, Color.White);
        }
    }
}