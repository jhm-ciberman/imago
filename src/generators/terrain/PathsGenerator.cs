using System;
using System.Numerics;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class PathsGenerator : IWorldGenerationStep
    {
        class TerrainPathfinder : Pathfinder
        {
            private float _flatTileCost = 1.0f;
            private float _slopeTileCost = 1.2f;
            private float _diagonalSlopeTileCost = 2.5f;
            private float _turnDirectionCost = 1.05f;
            private float _biomeChangeCost = 1.1f;

            private World _world;
            private float _heuristicMinWeight;

            public TerrainPathfinder(Tile start, Tile end, float quality = 1f) : base(start.world.size, start.coords, end.coords)
            {
                this._world = start.world;
                var minMax = this._world.minMaxNavgridCost;
                this._heuristicMinWeight = minMax.min + (minMax.max - minMax.min) * quality;
            }

            protected override float _HeuristicDistance(Vector2Int start, Vector2Int end)
            {
                var v = (end - start);
                return (System.Math.Abs(v.x) + System.Math.Abs(v.y)) * this._heuristicMinWeight;
            }

            private float _GetGradientCost(Tile tile, Vector2Int dir)
            {
                float h0 = tile.height0, h1 = tile.height1, h2 = tile.height2, h3 = tile.height3;
                var gradient = new Vector2((h1 + h3) - (h0 + h2), (h2 + h3) - (h0 + h1));

                if (gradient.X == 0f || gradient.Y == 0f) 
                {
                    if (gradient.X == 0f && gradient.Y == 0f)
                    {
                        return this._flatTileCost;
                    } 
                    else
                    {
                        if ((dir.x != 0 && gradient.Y != 0) || (dir.y != 0 && gradient.Y != 0))
                            return this._slopeTileCost;
                        else 
                            return this._diagonalSlopeTileCost;
                    }
                }
                return this._diagonalSlopeTileCost;
            }

            protected override float _WeightFunction(Vector2Int fromCoord, Vector2Int toCoord, Vector2Int cameFromCoord)
            {
                var toTile = this._world.GetTileAt(toCoord);
                var fromTile = this._world.GetTileAt(fromCoord);
                float weight = toTile.GetCellAtLevel(0).navgridCost;

                Vector2Int dir = (cameFromCoord - toCoord);
                if (dir.x != 0 && dir.y != 0) weight *= this._turnDirectionCost;

                if (fromTile.biome != toTile.biome) weight *= this._biomeChangeCost;

                weight *= this._GetGradientCost(toTile, dir);

                return weight;
            }
        }

        private System.Random _random;

        private IContainer _container;

        public PathsGenerator(IContainer container, int seed)
        {
            this._container = container;
            this._random = new System.Random(seed);

        }

        public void Handle(World world)
        {
            var center = world.size / 2;
            
            var tileCovers = new TileCover[] {
                this._container.Get<TileCover>("tilecover.mud"),
                this._container.Get<TileCover>("tilecover.stone"),
            };

            for(var i = 0; i < 5; i ++)
            {
                Tile start, end;
                int tries = 0;
                do
                {
                    if ((tries++) > 1000) { System.Console.WriteLine("Cannot create points"); return; }
                    int x1 = (int) MathF.Floor(((float)this._random.NextDouble()) * world.size.x);
                    int y1 = (int) MathF.Floor(((float)this._random.NextDouble()) * world.size.y);
                    int x2 = (int) MathF.Floor(((float)this._random.NextDouble()) * world.size.x);
                    int y2 = (int) MathF.Floor(((float)this._random.NextDouble()) * world.size.y);
                    start = world.GetTileAt(new Vector2Int(x1, y1));
                    end = world.GetTileAt(new Vector2Int(x2, y2));
                } while (! start.isWalkable || ! end.isWalkable);

                var pathfinder = new TerrainPathfinder(start, end);
                var list = pathfinder.Pathfind();
                if (list == null) 
                {
                    System.Console.WriteLine("Path not found");
                    continue;
                }

                var tilecover = tileCovers[this._random.Next(tileCovers.Length)];
                foreach (var coord in list)
                {
                    var tile = world.GetTileAt(coord);
                    //if (this._random.NextDouble() > 0.2)
                    //{
                        tile.SetTileCoverData(new TileCoverData(tilecover));

                    //}
                    //else
                    //{
                    //    tile.SetDecorationTilemap(tilecover);
                    //    tile.SetDecorationStyle(Tile.DecorationStyle.DiagonalBottomLeft);
                    //}
                    var cell = tile.GetCellAtLevel(0);
                    
                    cell.SetNavgridCost(cell.navgridCost * 0.75f);
                }

            }
        }
    }
}