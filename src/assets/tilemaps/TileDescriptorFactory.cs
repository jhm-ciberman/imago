using System.Linq;
using static LifeSim.Assets.Tilemap;
using static LifeSim.Assets.TileRequest;

namespace LifeSim.Assets
{
    public class TileDescriptorFactory
    {
        private AssetsContainer _assets; 

        public TileDescriptorFactory(AssetsContainer assetsContainer)
        {
            this._assets = assetsContainer;
        }

        public TileDescriptor BuildDescriptor(TileRequest request)
        {
            var centerTileMap = this._assets.GetTilemap(request.center);
            var descriptor = new TileDescriptor(centerTileMap);

            foreach (var entry in request.layers.OrderBy(entry => entry.Key.dominance))
            {
                var bytemask = entry.Value;
                var layerTileMap = this._assets.GetTilemap(entry.Key);
                this._AddLayers(descriptor, layerTileMap, bytemask);
            }

            return descriptor;
        }

        private void _AddLayers(TileDescriptor descriptor, Tilemap tilemap, Piece bytemask)
        {
            if (bytemask == Piece.None) return;

            if (tilemap.layout == Layout.Corners)
            {
                // Diagonals
                     if (bytemask.HasFlag(Piece.DiagonalTopLeft))     { bytemask &= ~Piece.DiagonalTopLeft;     descriptor.AddLayer(tilemap.layerDiagonalTopLeft);     }
                else if (bytemask.HasFlag(Piece.DiagonalTopRight))    { bytemask &= ~Piece.DiagonalTopRight;    descriptor.AddLayer(tilemap.layerDiagonalTopRight);    }
                else if (bytemask.HasFlag(Piece.DiagonalBottomLeft))  { bytemask &= ~Piece.DiagonalBottomLeft;  descriptor.AddLayer(tilemap.layerDiagonalBottomLeft);  }
                else if (bytemask.HasFlag(Piece.DiagonalBottomRight)) { bytemask &= ~Piece.DiagonalBottomRight; descriptor.AddLayer(tilemap.layerDiagonalBottomRight); }

                var left        = bytemask.HasFlag(Piece.Left);
                var right       = bytemask.HasFlag(Piece.Right);
                var top         = bytemask.HasFlag(Piece.Top);
                var bottom      = bytemask.HasFlag(Piece.Bottom);

                var topLeft     = bytemask.HasFlag(Piece.TopLeft);
                var bottomLeft  = bytemask.HasFlag(Piece.BottomLeft);
                var topRight    = bytemask.HasFlag(Piece.TopRight);
                var bottomRight = bytemask.HasFlag(Piece.BottomRight);

                // Laterals
                if (left   && ! top)    descriptor.AddLayer(tilemap.layerLateralLeftTopPart);
                if (left   && ! bottom) descriptor.AddLayer(tilemap.layerLateralLeftBottomPart);
                if (right  && ! top)    descriptor.AddLayer(tilemap.layerLateralRightTopPart);
                if (right  && ! bottom) descriptor.AddLayer(tilemap.layerLateralRightBottomPart);
                if (top    && ! left)   descriptor.AddLayer(tilemap.layerLateralTopLeftPart);
                if (top    && ! right)  descriptor.AddLayer(tilemap.layerLateralTopRightPart);
                if (bottom && ! left)   descriptor.AddLayer(tilemap.layerLateralBottomLeftPart);
                if (bottom && ! right)  descriptor.AddLayer(tilemap.layerLateralBottomRightPart);

                // Interior corners
                if (left   && top)      descriptor.AddLayer(tilemap.layerInteriorTopLeft);
                if (right  && top)      descriptor.AddLayer(tilemap.layerInteriorTopRight);
                if (left   && bottom)   descriptor.AddLayer(tilemap.layerInteriorBottomLeft);
                if (right  && bottom)   descriptor.AddLayer(tilemap.layerInteriorBottomRight);

                // Exterior corners
                if (! left  && ! top    && topLeft)     descriptor.AddLayer(tilemap.layerExteriorTopLeft);
                if (! left  && ! bottom && bottomLeft)  descriptor.AddLayer(tilemap.layerExteriorTopRight);
                if (! right && ! top    && topRight)    descriptor.AddLayer(tilemap.layerExteriorBottomLeft);
                if (! right && ! bottom && bottomRight) descriptor.AddLayer(tilemap.layerExteriorBottomRight);
            }
        }

    }
}