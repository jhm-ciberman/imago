using System.Collections.Generic;
using System;
using System.Linq;
using static LifeSim.Assets.TileRequest;
using static LifeSim.Assets.Tilemap;

namespace LifeSim.Assets
{
    public class TileDescriptor : IEquatable<TileDescriptor>
    {
        public readonly struct Layer
        {
            public readonly RectInt srcRect;
            public readonly Vector2Int dstOffset;
            public readonly Tilemap tilemap;

            public Layer(Tilemap tilemap, RectInt srcRect, Vector2Int dstOffset)
            {
                var pos    = (srcRect.coords * tilemap.tileSize);
                var size   = (srcRect.size   * tilemap.tileSize);
                var offset = (dstOffset      * tilemap.tileSize);

                this.tilemap = tilemap;
                this.dstOffset = offset;
                this.srcRect = new RectInt(pos, size);
            }

            public override string ToString() 
            {
                return "Layer(" + this.tilemap + "," + this.srcRect + "," + this.dstOffset + ")";
            }
        }

        public bool rotable;

        public TileDescriptor(Tilemap baseTilemap)
        {
            this._layers.Add(baseTilemap.layerBase);
            this.rotable = baseTilemap.centerRotable;
        }

        public override string ToString() 
        {
            string str = "TileDescriptor {";
            foreach (var layer in this.layers)
                str += layer.ToString() + ",";
            str += ")";
            return str;
        }

        List<Layer> _layers = new List<Layer>();

        public IEnumerable<Layer> layers => this._layers;
        public int layersCount => this._layers.Count;

        private void _AddLayer(Layer layer)
        {
            this._layers.Add(layer);
            this.rotable = false;
        }


        public void AddLayers(Tilemap tilemap, Piece bytemask)
        {
            if (bytemask == Piece.None) return;

            if (tilemap.layout == Layout.Corners)
            {
                // Diagonals
                     if (bytemask.HasFlag(Piece.DiagonalTopLeft))     { bytemask &= ~Piece.DiagonalTopLeft;     this._AddLayer(tilemap.layerDiagonalTopLeft);     }
                else if (bytemask.HasFlag(Piece.DiagonalTopRight))    { bytemask &= ~Piece.DiagonalTopRight;    this._AddLayer(tilemap.layerDiagonalTopRight);    }
                else if (bytemask.HasFlag(Piece.DiagonalBottomLeft))  { bytemask &= ~Piece.DiagonalBottomLeft;  this._AddLayer(tilemap.layerDiagonalBottomLeft);  }
                else if (bytemask.HasFlag(Piece.DiagonalBottomRight)) { bytemask &= ~Piece.DiagonalBottomRight; this._AddLayer(tilemap.layerDiagonalBottomRight); }

                var left        = bytemask.HasFlag(Piece.Left);
                var right       = bytemask.HasFlag(Piece.Right);
                var top         = bytemask.HasFlag(Piece.Top);
                var bottom      = bytemask.HasFlag(Piece.Bottom);

                var topLeft     = bytemask.HasFlag(Piece.TopLeft);
                var bottomLeft  = bytemask.HasFlag(Piece.BottomLeft);
                var topRight    = bytemask.HasFlag(Piece.TopRight);
                var bottomRight = bytemask.HasFlag(Piece.BottomRight);

                // Laterals
                if (left   && ! top)    this._AddLayer(tilemap.layerLateralLeftTopPart);
                if (left   && ! bottom) this._AddLayer(tilemap.layerLateralLeftBottomPart);
                if (right  && ! top)    this._AddLayer(tilemap.layerLateralRightTopPart);
                if (right  && ! bottom) this._AddLayer(tilemap.layerLateralRightBottomPart);
                if (top    && ! left)   this._AddLayer(tilemap.layerLateralTopLeftPart);
                if (top    && ! right)  this._AddLayer(tilemap.layerLateralTopRightPart);
                if (bottom && ! left)   this._AddLayer(tilemap.layerLateralBottomLeftPart);
                if (bottom && ! right)  this._AddLayer(tilemap.layerLateralBottomRightPart);

                // Interior corners
                if (left   && top)      this._AddLayer(tilemap.layerInteriorTopLeft);
                if (right  && top)      this._AddLayer(tilemap.layerInteriorTopRight);
                if (left   && bottom)   this._AddLayer(tilemap.layerInteriorBottomLeft);
                if (right  && bottom)   this._AddLayer(tilemap.layerInteriorBottomRight);

                // Exterior corners
                if (! left  && ! top    && topLeft)     this._AddLayer(tilemap.layerExteriorTopLeft);
                if (! left  && ! bottom && bottomLeft)  this._AddLayer(tilemap.layerExteriorTopRight);
                if (! right && ! top    && topRight)    this._AddLayer(tilemap.layerExteriorBottomLeft);
                if (! right && ! bottom && bottomRight) this._AddLayer(tilemap.layerExteriorBottomRight);
            }
        }


        public override int GetHashCode()
        {
            int hc = 0;
            foreach (var p in this._layers)
            {
                hc ^= p.GetHashCode();
            }
            return hc;
        }

        public bool Equals(TileDescriptor? other)
        {
            return (other != null && this._layers.SequenceEqual(other._layers));
        }
    }
}