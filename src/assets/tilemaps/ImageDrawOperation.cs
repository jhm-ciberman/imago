using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LifeSim.Assets
{
    public class UnpackedTexture : IDrawOperation
    {
        public string id;
        private readonly Image<Rgba32> _src;
        private readonly Vector2Int _size;

        public Vector2Int size => this._size;

        public UnpackedTexture(string id, string imagePath) 
        {
            this.id = id;
            this._src = Image.Load<Rgba32>(imagePath);
            this._size = new Vector2Int(this._src.Width, this._src.Height);
        }

        public void Draw(Image<Rgba32> dst, Vector2Int coord, Vector2Int size) 
        {
            int x = coord.x;
            int y = coord.y;
            int w = this._src.Width;
            int h = this._src.Height;
            int r = size.x - w;
            int b = size.y - h;

            if (r > 0) { // Right 
                this._DrawImage(dst, 
                    sx: w - 1, sy: 0, sw: 1, sh: h, 
                    dx: x + w, dy: y, dw: r, dh: h); 
            }

            if (b > 0) { // Bottom 
                this._DrawImage(dst, 
                    sx: 0 , sy: h - 1, sw: w, sh: 1, 
                    dx: x , dy: y + h, dw: w, dh: b);
            }
            
            if (r > 0 && b > 0) { // Bottom Right 
                this._DrawImage(dst, 
                    sx: w - 1, sy: h - 1, sw: 1, sh: 1,
                    dx: x + w, dy: y + h, dw: r, dh: b);
            }

            // The actual image:
            dst.Mutate(ctx => ctx.DrawImage(this._src, new Point(x, y), 1f));
        }

        // x = source, d = destination
        private void _DrawImage(Image<Rgba32> dst, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
        {
            float scaleX = sw / dw;
            float scaleY = sh / dh; 
            for (int xx = 0; xx < dw; xx++) {
                for (int yy = 0; yy < dh; yy++) {   
                    int destX = dx + xx;
                    int destY = dy + yy;
                    if (destX >= 0 && destY >= 0 && destX < dst.Width && destY < dst.Height) {
                        var col = dst[sx + (int) (xx * scaleX), sy + (int) (yy * scaleY)];
                        dst[destX, destY] = col;
                    }
                }
            }
        }
    }
}
