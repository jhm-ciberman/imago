using System.Numerics;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Frame2D : RenderNode2D, ICanvasItem
    {
        public Texture texture;

        public Vector2 size;

        public Frame2D(Texture texture) : this(texture, new Vector2(texture.width, texture.height)) { }

        public Frame2D(Texture texture, Vector2 size)
        {
            this.texture = texture;
            this.size = size;
        }

        public override void Render(SpriteBatcher spriteBatcher)
        {
            spriteBatcher.Draw(this.texture, this.worldMatrix.Translation, this.size);
        }
    }
}