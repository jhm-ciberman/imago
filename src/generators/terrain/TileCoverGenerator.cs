using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class TileCoverGenerator : IWorldGenerationStep
    {
        public void Handle(World world)
        {
            foreach (Tile tile in world.tiles)
            {
                tile.SetTileCoverData(this._GetTileCoverData(tile));
            }
        }


        private TileCoverData _GetTileCoverData(Tile tile)
        {
            var cover = tile.baseTileCover;
            var north = tile.north.baseTileCover ?? cover;
            var east  = tile.east.baseTileCover  ?? cover;
            var south = tile.south.baseTileCover ?? cover;
            var west  = tile.west.baseTileCover  ?? cover;

            var dominance = cover.dominance;
            var northDiff = ! (cover == north) && dominance < north.dominance;
            var eastDiff  = ! (cover == east ) && dominance < east.dominance;
            var southDiff = ! (cover == south) && dominance < south.dominance;
            var westDiff  = ! (cover == west ) && dominance < west.dominance;
            
            var count = (northDiff ? 1 : 0) + (eastDiff ? 1 : 0) + (southDiff ? 1 : 0) + (westDiff ? 1 : 0);

            if (count == 2)
            {
                     if (northDiff && eastDiff  && north == east ) return new TileCoverData(cover, north, TileCoverData.DecorationStyle.DiagonalTopRight);
                else if (eastDiff  && southDiff && east  == south) return new TileCoverData(cover, east , TileCoverData.DecorationStyle.DiagonalBottomRight);
                else if (southDiff && westDiff  && south == west ) return new TileCoverData(cover, south, TileCoverData.DecorationStyle.DiagonalBottomLeft);
                else if (westDiff  && northDiff && west  == north) return new TileCoverData(cover, west , TileCoverData.DecorationStyle.DiagonalTopLeft);
            }

            return new TileCoverData(cover);
        }
    }

}