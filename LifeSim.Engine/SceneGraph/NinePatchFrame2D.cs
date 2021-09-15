using System;
using System.Numerics;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class NinePatchFrame2D : Frame2D, ICanvasItem
    {
        public bool drawCenter { get; set; } = true;
        public int patchMarginTop { get; set; } 
        public int patchMarginBottom { get; set; } 
        public int patchMarginLeft { get; set; } 
        public int patchMarginRight { get; set; } 

        public Color color { get; set; } = Color.white;

        public NinePatchFrame2D(Texture texture) : base(texture, new Vector2(texture.width, texture.height)) { }

        public NinePatchFrame2D(Texture texture, Vector2 size) : base(texture, size)
        {
            this.texture = texture;
            this.size = size;
        }

        public override void Render(SpriteBatcher spriteBatcher)
        {
            if (this.texture == null) return;
            if (this.size.X <= 0 || this.size.Y <= 0) return;

            var sizeTotal = new Vector2(this.texture.width, this.texture.height);
            var sizeTL = new Vector2(this.patchMarginLeft, this.patchMarginTop);
            var sizeBR = new Vector2(this.patchMarginRight, this.patchMarginBottom);
            var sizeTR = new Vector2(this.patchMarginTop, this.patchMarginRight);
            var sizeBL = new Vector2(this.patchMarginLeft, this.patchMarginBottom);



            var uvTL = sizeTL / sizeTotal;
            var uvBR = Vector2.One - sizeBR / sizeTotal;

            float scale = 2f;

            // Scale if the size is smaller (after calculating UVs)
            var minimumRequiredSize = (sizeTL + sizeBR) * scale;
            if (minimumRequiredSize.X > 0 && minimumRequiredSize.Y > 0) {
                if (this.size.X < minimumRequiredSize.X || this.size.Y < minimumRequiredSize.Y) {
                    scale *= MathF.Min(this.size.X, this.size.Y) / MathF.Min(minimumRequiredSize.X, minimumRequiredSize.Y);
                }
            }

            sizeTL *= scale;
            sizeBR *= scale;
            sizeTR *= scale;
            sizeBL *= scale;
            var sizeSegmentCenter = this.size - sizeTL - sizeBR;

            ref Matrix3x2 worldMatrix = ref this.worldMatrix;
            float depth = 0f; 

            if (this.drawCenter) {

                if (sizeSegmentCenter.X > 0 && sizeSegmentCenter.Y > 0) {
                    spriteBatcher.Draw(this.texture, -this.pivot + sizeTL, sizeSegmentCenter, uvTL, uvBR, in worldMatrix, this.color, depth);
                }
            }

            var posTL = new Vector2(0f, 0f);
            var posTR = new Vector2(this.size.X - sizeTL.X, 0f                );
            var posBR = new Vector2(this.size.X - sizeTL.X, this.size.Y - sizeTL.Y);
            var posBL = new Vector2(0f                , this.size.Y - sizeTL.Y);

            // Corner Top Left
            spriteBatcher.Draw(this.texture, -this.pivot + posTL, sizeTL, Vector2.Zero, uvTL, in worldMatrix, this.color, depth);
            // Corner Top Right
            spriteBatcher.Draw(this.texture, -this.pivot + posTR, sizeTR, new Vector2(uvBR.X, 0f), new Vector2(1f, uvTL.Y), in worldMatrix, this.color, depth);
            // Corner Bottom Left
            spriteBatcher.Draw(this.texture, -this.pivot + posBL, sizeBL, new Vector2(0f, uvBR.Y), new Vector2(uvTL.X, 1f), in worldMatrix, this.color, depth);
            // Corner Bottom Right
            spriteBatcher.Draw(this.texture, -this.pivot + posBR, sizeBR, uvBR, Vector2.One, in worldMatrix, this.color, depth);


            var sizeTop = new Vector2(this.size.X - sizeTL.X - sizeTR.X, sizeTL.Y);
            var sizeBottom = new Vector2(this.size.X - sizeBL.X - sizeBR.X, sizeBL.Y);
            var sizeLeft = new Vector2(sizeTL.X, this.size.Y - sizeTL.Y - sizeBL.Y);
            var sizeRight = new Vector2(sizeTR.X, this.size.Y - sizeTR.Y - sizeBR.Y);

            // Lateral Top
            spriteBatcher.Draw(this.texture, -this.pivot + new Vector2(sizeTL.X, 0f), sizeTop, new Vector2(uvTL.X, 0f), new Vector2(uvBR.X, uvTL.Y), in worldMatrix, this.color, depth);
            // Lateral Bottom
            spriteBatcher.Draw(this.texture, -this.pivot + new Vector2(sizeBL.X, this.size.Y - sizeBL.Y), sizeBottom, new Vector2(uvTL.X, uvBR.Y), new Vector2(uvBR.X, 1f), in worldMatrix, this.color, depth);
            // Lateral Left
            spriteBatcher.Draw(this.texture, -this.pivot + new Vector2(0f, sizeTL.Y), sizeLeft, new Vector2(0f, uvTL.Y), new Vector2(uvTL.X, uvBR.Y), in worldMatrix, this.color, depth);
            // Lateral Right
            spriteBatcher.Draw(this.texture, -this.pivot + new Vector2(this.size.X - sizeTR.X, sizeTR.Y), sizeRight, new Vector2(uvBR.X, uvTL.Y), new Vector2(1f, uvBR.Y), in worldMatrix, this.color, depth);
        }
    }
}