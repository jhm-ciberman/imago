using System.Collections.Generic;
using System.Linq;
using static LifeSim.Assets.TileDescriptor;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace LifeSim.Assets
{
    public class TileDrawOperation
    {
        private int _tileSize;
        private Image<Rgba32> _image;
        private Vector2Int _dstCoord;

        private IEnumerable<Layer> _layers;

        public TileDrawOperation(TileDescriptor descriptor, int tileSize, Image<Rgba32> image, Vector2Int dstCoord)
        {
            this._layers = descriptor.layers;
            this._tileSize = tileSize;
            this._image = image;
            this._dstCoord = dstCoord;

            this._dstCoord.x *= this._tileSize;
            this._dstCoord.y *= this._tileSize;
        }

        public void DrawTile()
        {
            for(int x = 0; x < this._tileSize; x++)
            {
                for(int y = 0; y < this._tileSize; y++)
                {
                    Vector2Int pixel = new Vector2Int(x, y);
                    var col = this._Fragment(pixel);
                    int px = this._dstCoord.x + pixel.x;
                    int py = this._dstCoord.y + pixel.y;
                    this._image[px, py] = col;
                }
            }
        }

        private Rgba32 _Fragment(Vector2Int pixel)
        {
            var firstLayer = this._layers.First();
            var topTilemap = firstLayer.tilemap;

            Rgba32 outCol = this._Sample(firstLayer, pixel);
            
            foreach (var layer in this._layers.Skip(1))
            {
                if (! this._IsInsideLayer(pixel, layer)) continue;

                Rgba32 col = this._Sample(layer, pixel);

                if (col.A == 0f) continue; // Discard pixel

                // The alpha channel is used as LUT index
                if (col.R == 255 && col.G == 0 && col.B == 255) { // Pure magenta color
                    var topShadeColor = topTilemap.lut.GetShadeColor(col.A / 255f);
                    var currentLayerDarkestColor = layer.tilemap.lut.GetShadeColor(1f);
                    outCol = this._Blend(currentLayerDarkestColor, topShadeColor, topShadeColor.A / 255f);
                } else {
                    outCol = col;   
                    topTilemap = layer.tilemap;
                }

            }

            return outCol;
        }

        private Rgba32 _Blend(Rgba32 colA, Rgba32 colB, float amount)
        {
            return new Rgba32(
                (byte) (colA.R + (colB.R - colA.R) * amount),
                (byte) (colA.G + (colB.G - colA.G) * amount),
                (byte) (colA.B + (colB.B - colA.B) * amount),
                (byte) (colA.A + (colB.A - colA.A) * amount)
            );
        }

        private bool _IsInsideLayer(Vector2Int pixel, Layer layer)
        {
            return (
                   pixel.x >= layer.dstOffset.x 
                && pixel.y >= layer.dstOffset.y 
                && pixel.x <  layer.dstOffset.x + layer.srcRect.width 
                && pixel.y <  layer.dstOffset.y + layer.srcRect.height
            );
        }

        private Color _Sample(Layer layer, Vector2Int pixel)
        {
            var p = layer.srcRect.coords + pixel - layer.dstOffset;
            var texture = layer.tilemap.image;
            return texture[p.x, p.y];
        }

    }
}