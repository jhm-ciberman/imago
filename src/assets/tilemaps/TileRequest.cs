using System.Collections.Generic;
using static LifeSim.Assets.Tilemap;
using System.Linq;
using System;
using static LifeSim.Simulation.TileCoverData;
using LifeSim.Simulation;

namespace LifeSim.Assets
{
    public class TileRequest : IEquatable<TileRequest>
    {
        [Flags]
        public enum Piece
        {
            None                = 0,
            TopLeft             = 1 << 0,
            Top                 = 1 << 1,
            TopRight            = 1 << 2,
            Left                = 1 << 3,
            Center              = 1 << 4,
            Right               = 1 << 5,
            BottomLeft          = 1 << 6,
            Bottom              = 1 << 7,
            BottomRight         = 1 << 8,
            DiagonalTopLeft     = (Piece.Top    | Piece.Left  | Piece.TopLeft     | Piece.TopRight    | Piece.BottomLeft  | Piece.Center),
            DiagonalBottomLeft  = (Piece.Bottom | Piece.Left  | Piece.BottomLeft  | Piece.BottomRight | Piece.TopLeft     | Piece.Center),
            DiagonalTopRight    = (Piece.Top    | Piece.Right | Piece.TopRight    | Piece.TopLeft     | Piece.BottomRight | Piece.Center),
            DiagonalBottomRight = (Piece.Bottom | Piece.Right | Piece.BottomRight | Piece.TopRight    | Piece.BottomLeft  | Piece.Center),
        }

        public TileCover center;

        private Dictionary<TileCover, Piece> _bytemasks = new Dictionary<TileCover, Piece>();

        public IEnumerable<KeyValuePair<TileCover, Piece>> layers => this._bytemasks;

        public TileRequest(Tile tile)
        {
            var cover = tile.cover;
            this.center = cover.baseTileCover;

            foreach (var check in TileRequest._checks)
            {
                var neighbour = this.FindNeighbour(tile, check.coord);
            
                var tilecover = (neighbour is Tile opositeTile) 
                    ? this._ResolveTilemap(opositeTile, check.opositePiece)
                    : this._ResolveTilemap(tile, check.piece);

                this.AddPiece(tilecover, check.piece);
            }

            var decorationPiece = this._GetDecorationPiece(cover.decorationStyle);
            this.AddPiece(cover.decorationTileCover, decorationPiece);
        }

        protected struct TileCheck
        {
            public Vector2Int coord;
            public Piece piece;
            public Piece opositePiece;

            public TileCheck(Vector2Int coord, Piece piece, Piece opositePiece)
            {
                this.coord = coord;
                this.piece = piece;
                this.opositePiece = opositePiece;
            }
        }

        // TODO: Maybe all these checks can be optimized. But for now it's ok
        protected static TileCheck[] _checks = new TileCheck[] {
            new TileCheck(new Vector2Int( 0,  1), Piece.Top        , Piece.Bottom     ),
            new TileCheck(new Vector2Int(-1,  0), Piece.Left       , Piece.Right      ),
            new TileCheck(new Vector2Int( 1,  0), Piece.Right      , Piece.Left       ),
            new TileCheck(new Vector2Int( 0, -1), Piece.Bottom     , Piece.Top        ),

            new TileCheck(new Vector2Int(-1, -1), Piece.BottomLeft , Piece.TopRight   ),
            new TileCheck(new Vector2Int( 1, -1), Piece.BottomRight, Piece.TopLeft    ),
            new TileCheck(new Vector2Int(-1,  1), Piece.TopLeft    , Piece.BottomRight),
            new TileCheck(new Vector2Int( 1,  1), Piece.TopRight   , Piece.BottomLeft ),

            new TileCheck(new Vector2Int(-1,  0), Piece.TopLeft    , Piece.Top   ),
            new TileCheck(new Vector2Int( 0,  1), Piece.TopLeft    , Piece.Left  ),

            new TileCheck(new Vector2Int( 1,  0), Piece.TopRight   , Piece.Top   ),
            new TileCheck(new Vector2Int( 0,  1), Piece.TopRight   , Piece.Right ),

            new TileCheck(new Vector2Int(-1,  0), Piece.BottomLeft , Piece.Bottom),
            new TileCheck(new Vector2Int( 0, -1), Piece.BottomLeft , Piece.Left  ),

            new TileCheck(new Vector2Int( 1,  0), Piece.BottomRight, Piece.Bottom),
            new TileCheck(new Vector2Int( 0, -1), Piece.BottomRight, Piece.Right ),
        };

        public Tile? FindNeighbour(Tile tile, Vector2Int delta)
        {
            var coord = tile.coords + delta;

            return (tile.world.TileCoordIsInside(coord.x, coord.y))
                ? tile.world.GetTileAt(coord)
                : (Tile?) null;
        }

        public TileCover _ResolveTilemap(Tile tile, Piece piece)
        {
            var cover = tile.cover;
            var decorationPiece = this._GetDecorationPiece(cover.decorationStyle);
            return ((piece & decorationPiece) > 0 && cover.decorationTileCover != null) 
                ? cover.decorationTileCover 
                : cover.baseTileCover;
        }

        private Piece _GetDecorationPiece(DecorationStyle decorationStyle)
        {
            switch (decorationStyle)
            {
                case DecorationStyle.DiagonalTopLeft:     return Piece.DiagonalTopLeft;
                case DecorationStyle.DiagonalBottomLeft:  return Piece.DiagonalBottomLeft;
                case DecorationStyle.DiagonalTopRight:    return Piece.DiagonalTopRight;
                case DecorationStyle.DiagonalBottomRight: return Piece.DiagonalBottomRight;
            }
            return Piece.None;
        }

        public void AddPiece(TileCover? tilemap, Piece piece)
        {
            if (tilemap == null) return;
            if (this.center == tilemap) return;
            if (this.center.dominance > tilemap.dominance) return;

            Piece bytemask = 0;
            this._bytemasks.TryGetValue(tilemap, out bytemask);
            bytemask |= piece;
            this._bytemasks[tilemap] = bytemask;
        }

        public override int GetHashCode()
        {
            int hc = 0;
            foreach (var p in this._bytemasks)
            {
                hc ^= p.GetHashCode();
            }
            return hc ^ this.center.GetHashCode();
        }

        public bool Equals(TileRequest? other)
        {
            return (other != null && this._bytemasks.SequenceEqual(other._bytemasks) && this.center == other.center);
        }
    }
}