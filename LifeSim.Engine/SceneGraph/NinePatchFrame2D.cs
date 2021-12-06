using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public class NinePatchFrame2D : Frame2D, ICanvasItem
    {
        public bool DrawCenter { get; set; } = true;
        public int PatchMarginTop { get; set; }
        public int PatchMarginBottom { get; set; }
        public int PatchMarginLeft { get; set; }
        public int PatchMarginRight { get; set; }

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

            var sizeTotal = new Vector2(this.Texture.Width, this.Texture.Height);
            var sizeTL = new Vector2(this.PatchMarginLeft, this.PatchMarginTop);
            var sizeBR = new Vector2(this.PatchMarginRight, this.PatchMarginBottom);
            var sizeTR = new Vector2(this.PatchMarginTop, this.PatchMarginRight);
            var sizeBL = new Vector2(this.PatchMarginLeft, this.PatchMarginBottom);



            var uvTL = sizeTL / sizeTotal;
            var uvBR = Vector2.One - sizeBR / sizeTotal;

            float scale = 2f;

            // Scale if the size is smaller (after calculating UVs)
            var minimumRequiredSize = (sizeTL + sizeBR) * scale;
            if (minimumRequiredSize.X > 0 && minimumRequiredSize.Y > 0)
            {
                if (this.Size.X < minimumRequiredSize.X || this.Size.Y < minimumRequiredSize.Y)
                {
                    scale *= MathF.Min(this.Size.X, this.Size.Y) / MathF.Min(minimumRequiredSize.X, minimumRequiredSize.Y);
                }
            }

            sizeTL *= scale;
            sizeBR *= scale;
            sizeTR *= scale;
            sizeBL *= scale;
            var sizeSegmentCenter = this.Size - sizeTL - sizeBR;

            ref Matrix3x2 worldMatrix = ref this.WorldMatrix;
            float depth = 0f;

            if (this.DrawCenter)
            {

                if (sizeSegmentCenter.X > 0 && sizeSegmentCenter.Y > 0)
                {
                    spriteBatcher.Draw(this.Texture, -this.Pivot + sizeTL, sizeSegmentCenter, uvTL, uvBR, in worldMatrix, this.Color, depth);
                }
            }

            var posTL = new Vector2(0f, 0f);
            var posTR = new Vector2(this.Size.X - sizeTL.X, 0f                );
            var posBR = new Vector2(this.Size.X - sizeTL.X, this.Size.Y - sizeTL.Y);
            var posBL = new Vector2(0f                , this.Size.Y - sizeTL.Y);

            // Corner Top Left
            spriteBatcher.Draw(this.Texture, -this.Pivot + posTL, sizeTL, Vector2.Zero, uvTL, in worldMatrix, this.Color, depth);
            // Corner Top Right
            spriteBatcher.Draw(this.Texture, -this.Pivot + posTR, sizeTR, new Vector2(uvBR.X, 0f), new Vector2(1f, uvTL.Y), in worldMatrix, this.Color, depth);
            // Corner Bottom Left
            spriteBatcher.Draw(this.Texture, -this.Pivot + posBL, sizeBL, new Vector2(0f, uvBR.Y), new Vector2(uvTL.X, 1f), in worldMatrix, this.Color, depth);
            // Corner Bottom Right
            spriteBatcher.Draw(this.Texture, -this.Pivot + posBR, sizeBR, uvBR, Vector2.One, in worldMatrix, this.Color, depth);


            var sizeTop = new Vector2(this.Size.X - sizeTL.X - sizeTR.X, sizeTL.Y);
            var sizeBottom = new Vector2(this.Size.X - sizeBL.X - sizeBR.X, sizeBL.Y);
            var sizeLeft = new Vector2(sizeTL.X, this.Size.Y - sizeTL.Y - sizeBL.Y);
            var sizeRight = new Vector2(sizeTR.X, this.Size.Y - sizeTR.Y - sizeBR.Y);

            // Lateral Top
            spriteBatcher.Draw(this.Texture, -this.Pivot + new Vector2(sizeTL.X, 0f), sizeTop, new Vector2(uvTL.X, 0f), new Vector2(uvBR.X, uvTL.Y), in worldMatrix, this.Color, depth);
            // Lateral Bottom
            spriteBatcher.Draw(this.Texture, -this.Pivot + new Vector2(sizeBL.X, this.Size.Y - sizeBL.Y), sizeBottom, new Vector2(uvTL.X, uvBR.Y), new Vector2(uvBR.X, 1f), in worldMatrix, this.Color, depth);
            // Lateral Left
            spriteBatcher.Draw(this.Texture, -this.Pivot + new Vector2(0f, sizeTL.Y), sizeLeft, new Vector2(0f, uvTL.Y), new Vector2(uvTL.X, uvBR.Y), in worldMatrix, this.Color, depth);
            // Lateral Right
            spriteBatcher.Draw(this.Texture, -this.Pivot + new Vector2(this.Size.X - sizeTR.X, sizeTR.Y), sizeRight, new Vector2(uvBR.X, uvTL.Y), new Vector2(1f, uvBR.Y), in worldMatrix, this.Color, depth);
        }
    }
}