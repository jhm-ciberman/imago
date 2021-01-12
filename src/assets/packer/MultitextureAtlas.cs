using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace LifeSim.Assets
{
    class AtlasBuilder 
    {
        public readonly int width;

        public readonly int height;

        private readonly Image<Rgba32> _texture;

        private int _mipMapLevels;

        public AtlasBuilder(int size, int mipMapLevels) 
        {
            this.width = size;
            this.height = size;
            this._mipMapLevels = mipMapLevels > 0 ? mipMapLevels : 0;
            this._texture = new Image<Rgba32>(size, size);
        }

        public Image<Rgba32> image => this._texture;

        private int _GetFillArea(int size)
        {
            return (((size - 1) >> this._mipMapLevels) + 1) << this._mipMapLevels;
        }

        public void Draw(Image<Rgba32> texture, Vector2Int coord) 
        {
            int x = coord.x << this._mipMapLevels;
            int y = coord.y << this._mipMapLevels;
            int w = texture.Width;
            int h = texture.Height;
            int r = this._GetFillArea(w) - w;
            int b = this._GetFillArea(h) - h;

            if (r > 0) // Right
            {
                this._DrawImage(texture, 
                    sx: w - 1, sy: 0, sw: 1, sh: h, 
                    dx: x + w, dy: y, dw: r, dh: h); 
            }

            if (b > 0)  // Bottom
            {
                this._DrawImage(texture, 
                    sx: 0 , sy: h - 1, sw: w, sh: 1, 
                    dx: x , dy: y + h, dw: w, dh: b);
            }
            
            if (r > 0 && b > 0) // Bottom Right
            {
                this._DrawImage(texture, 
                    sx: w - 1, sy: h - 1, sw: 1, sh: 1,
                    dx: x + w, dy: y + h, dw: r, dh: b);
            }
            // The actual image:
            this._texture.Mutate(ctx => ctx.DrawImage(texture, new Point(x, y), 1f));
        }

        // x = source, d = destination
        private void _DrawImage(Image<Rgba32> srcTexture, int sx, int sy, int sw, int sh, int dx, int dy, int dw, int dh)
        {
            float scaleX = sw / dw;
            float scaleY = sh / dh; 
            for (int xx = 0; xx < dw; xx++)
            {
                for (int yy = 0; yy < dh; yy++) 
                {   
                    int destX = dx + xx;
                    int destY = dy + yy;
                    if (destX >= 0 && destY >= 0 && destX < this.width && destY < this.height) 
                    {
                        var col = srcTexture[sx + (int) (xx * scaleX), sy + (int) (yy * scaleY)];
                        this._texture[destX, destY] = col;
                    }
                }
            }
        }
    }
}
