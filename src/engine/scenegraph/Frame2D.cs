using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class Frame2D : Renderable2D
    {
        public GPUTexture texture;

        public Vector2 size;

        public Frame2D(GPUTexture texture) : this(texture, new Vector2(texture.width, texture.height)) { }

        public Frame2D(GPUTexture texture, Vector2 size)
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