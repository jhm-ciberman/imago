using System.Numerics;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Frame2D : RenderNode2D, ICanvasItem
    {
        public Texture? Texture { get; set; }

        public Vector2 Size { get; set; }

        private Vector2 _pivot = Vector2.Zero;
        public Vector2 Pivot
        {
            get => this._pivot;
            set { this._pivot = value; this._OnTransformDirty(); }
        }

        public Frame2D() { }
        public Frame2D(Texture texture) : this(texture, new Vector2(texture.Width, texture.Height)) { }

        public Frame2D(Texture texture, Vector2 size)
        {
            this.Texture = texture;
            this.Size = size;
        }

        public override void Render(SpriteBatcher spriteBatcher)
        {
            if (this.Texture != null)
            {
                spriteBatcher.Draw(this.Texture, -this.Pivot, this.Size, Vector2.Zero, Vector2.One, in this.WorldMatrix, Color.White, 0f);
            }
        }
    }
}