using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LifeSim.Assets
{
    public class ImageDrawOperation 
    {
        private readonly Image<Rgba32> _src;

        private readonly Image<Rgba32> _dst;

        private readonly Vector2Int _coord;

        private readonly uint _tileSize;

        public ImageDrawOperation(Image<Rgba32> dst, Image<Rgba32> src, Vector2Int coord, uint tileSize) 
        {
            this._dst = dst;
            this._src = src;
            this._coord = coord;
            this._tileSize = tileSize;
        }

        private int _GetFillArea(int size)
        {
            return (int) (MathF.Ceiling(size / this._tileSize) * this._tileSize);
        }

        public void Draw() 
        {
            int x = this._coord.x * (int) this._tileSize;
            int y = this._coord.y * (int) this._tileSize;
            int w = this._src.Width;
            int h = this._src.Height;
            int r = this._GetFillArea(w) - w;
            int b = this._GetFillArea(h) - h;

            if (r > 0) { // Right 
                this._DrawImage(
                    sx: w - 1, sy: 0, sw: 1, sh: h, 
                    dx: x + w, dy: y, dw: r, dh: h); 
            }

            if (b > 0) { // Bottom 
                this._DrawImage(
                    sx: 0 , sy: h - 1, sw: w, sh: 1, 
                    dx: x , dy: y + h, dw: w, dh: b);
            }
            
            if (r > 0 && b > 0) { // Bottom Right 
                this._DrawImage( 
                    sx: w - 1, sy: h - 1, sw: 1, sh: 1,
                    dx: x + w, dy: y + h, dw: r, dh: b);
            }

            // The actual image:
            this._dst.Mutate(ctx => ctx.DrawImage(this._src, new Point(x, y), 1f));
        }

        // x = source, d = destination
        private void _DrawImage(int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
        {
            float scaleX = sw / dw;
            float scaleY = sh / dh; 
            for (int xx = 0; xx < dw; xx++) {
                for (int yy = 0; yy < dh; yy++) {   
                    int destX = dx + xx;
                    int destY = dy + yy;
                    if (destX >= 0 && destY >= 0 && destX < this._dst.Width && destY < this._dst.Height) {
                        var col = this._dst[sx + (int) (xx * scaleX), sy + (int) (yy * scaleY)];
                        this._dst[destX, destY] = col;
                    }
                }
            }
        }
    }
}
