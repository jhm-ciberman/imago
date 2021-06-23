using System.Numerics;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Frame2D : RenderNode2D, ICanvasItem
    {
        public Texture texture;

        public Vector2 size;

        private Vector2 _pivot = Vector2.Zero;
        public Vector2 pivot 
        { 
            get => this._pivot; 
            set { this._pivot = value; this._OnTransformDirty(); }
        }

        public Frame2D(Texture texture) : this(texture, new Vector2(texture.width, texture.height)) { }

        public Frame2D(Texture texture, Vector2 size)
        {
            this.texture = texture;
            this.size = size;
        }

        public override void Render(SpriteBatcher spriteBatcher)
        {
            spriteBatcher.Draw(this.texture, -this.pivot, this.size, in this.worldMatrix, Color.white, 0f);
        }
    }
}