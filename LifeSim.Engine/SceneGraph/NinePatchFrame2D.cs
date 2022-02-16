using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph;

public class NinePatchFrame2D : Frame2D, ICanvasItem
{
    public bool DrawCenter { get; set; } = true;
    public Thickness PatchMargin { get; set; } = new Thickness(0);

    public Color Color { get; set; } = Color.White;

    public NinePatchFrame2D(Texture texture) : base(texture, new Vector2(texture.Width, texture.Height)) { }

    public NinePatchFrame2D(Texture texture, Vector2 size) : base(texture, size)
    {
        this.Texture = texture;
        this.Size = size;
    }

    public override void Render(SpriteBatcher spriteBatcher)
    {
        if (this.Texture == null) return;
        if (this.Size.X <= 0 || this.Size.Y <= 0) return;

        spriteBatcher.DrawNinePatch(
            this.Shader,
            this.Texture,
            this.PatchMargin,
            this.Size,
            this.Pivot,
            ref this.WorldMatrix,
            this.Color,
            this.DrawCenter
        );
    }
}