using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static LifeSim.Assets.TileDescriptor;

namespace LifeSim.Assets
{
    public class Tilemap : IAsset
    {
        public Image<Rgba32> texture;

        public enum Layout
        {
            Simple,
            Corners,
        }

        public readonly Layout layout;

        public readonly int tileSize;

        public readonly TilemapLUT lut;

        public readonly bool centerRotable;

        public Tilemap(Image<Rgba32> texture, Layout layout, int tileSize, bool centerRotable)
        {
            this.texture = texture;
            this.layout = layout;
            this.tileSize = tileSize;
            this.centerRotable = centerRotable;

            this.lut = new TilemapLUT(texture, this.layerBase.srcRect);
        }

        public Layer layerBase                   => new Layer(this, new RectInt(0, 0, 4, 4) , new Vector2Int(0, 0));

        public Layer layerDiagonalTopLeft        => new Layer(this, new RectInt(12, 4, 4, 4), new Vector2Int(0, 0));
        public Layer layerDiagonalTopRight       => new Layer(this, new RectInt( 8, 4, 4, 4), new Vector2Int(0, 0)); 
        public Layer layerDiagonalBottomLeft     => new Layer(this, new RectInt(12, 0, 4, 4), new Vector2Int(0, 0)); 
        public Layer layerDiagonalBottomRight    => new Layer(this, new RectInt( 8, 0, 4, 4), new Vector2Int(0, 0)); 

        public Layer layerInteriorTopLeft        => new Layer(this, new RectInt(4, 4, 2, 2) , new Vector2Int(0, 0));
        public Layer layerInteriorTopRight       => new Layer(this, new RectInt(6, 4, 2, 2) , new Vector2Int(2, 0));
        public Layer layerInteriorBottomLeft     => new Layer(this, new RectInt(4, 6, 2, 2) , new Vector2Int(0, 2));
        public Layer layerInteriorBottomRight    => new Layer(this, new RectInt(6, 6, 2, 2) , new Vector2Int(2, 2));

        public Layer layerExteriorTopLeft        => new Layer(this, new RectInt(3, 5, 1, 1) , new Vector2Int(0, 0));
        public Layer layerExteriorTopRight       => new Layer(this, new RectInt(3, 4, 1, 1) , new Vector2Int(0, 3));
        public Layer layerExteriorBottomLeft     => new Layer(this, new RectInt(2, 5, 1, 1) , new Vector2Int(3, 0));
        public Layer layerExteriorBottomRight    => new Layer(this, new RectInt(2, 4, 1, 1) , new Vector2Int(3, 3));

        public Layer layerLateralLeftTopPart     => new Layer(this, new RectInt(0, 4, 1, 2) , new Vector2Int(0, 0));
        public Layer layerLateralLeftBottomPart  => new Layer(this, new RectInt(0, 6, 1, 2) , new Vector2Int(0, 2));
        public Layer layerLateralRightTopPart    => new Layer(this, new RectInt(1, 4, 1, 2) , new Vector2Int(3, 0));
        public Layer layerLateralRightBottomPart => new Layer(this, new RectInt(1, 6, 1, 2) , new Vector2Int(3, 2));
        public Layer layerLateralTopLeftPart     => new Layer(this, new RectInt(4, 0, 2, 1) , new Vector2Int(0, 0));
        public Layer layerLateralTopRightPart    => new Layer(this, new RectInt(6, 0, 2, 1) , new Vector2Int(2, 0));
        public Layer layerLateralBottomLeftPart  => new Layer(this, new RectInt(4, 1, 2, 1) , new Vector2Int(0, 3));
        public Layer layerLateralBottomRightPart => new Layer(this, new RectInt(6, 1, 2, 1) , new Vector2Int(2, 3));
    }
}