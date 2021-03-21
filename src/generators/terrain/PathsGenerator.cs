using System;
using System.Numerics;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class PathsGenerator : IWorldGenerationStep
    {
        private class TerrainNavigator : INavigator<Tile>
        {
            private readonly float _flatTileCost = 1.0f;
            private readonly float _slopeTileCost = 1.2f;
            private readonly float _diagonalSlopeTileCost = 2.5f;
            private readonly float _turnDirectionCost = 1.05f;
            private readonly float _biomeChangeCost = 1.1f;

            private readonly float _heuristicMinWeight;

            private Vector2Int _size;

            public TerrainNavigator(World world, float quality = 1f)
            {
                var minMax = world.minMaxNavgridCost;
                this._size = world.size;
                this._heuristicMinWeight = minMax.min + (minMax.max - minMax.min) * quality;
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

            float INavigator<Tile>.HeuristicDistance(Tile start, Tile end)
            {
                var v = (end.coords - start.coords);
                return (System.Math.Abs(v.x) + System.Math.Abs(v.y)) * this._heuristicMinWeight;
            }

            float INavigator<Tile>.WeightFunction(Tile fromTile, Tile toTile, Tile cameFromTile)
            {
                float weight = toTile.baseCell.navgridCost;

                Vector2Int dir = (cameFromTile.coords - toTile.coords);
                if (dir.x != 0 && dir.y != 0) weight *= this._turnDirectionCost;

                if (fromTile.biome != toTile.biome) weight *= this._biomeChangeCost;

                weight *= this._GetGradientCost(toTile, dir);

                return weight;
            }

            void INavigator<Tile>.VisitNodeNeighbours(INodeVisitor<Tile> nodeVisitor, Tile node)
            {
                var coords = node.coords;
                if (coords.x - 1 >= 0)
                    nodeVisitor.VisitNode(node.west);

                if (coords.x + 1 < this._size.x)
                    nodeVisitor.VisitNode(node.east);

                if (coords.y + 1 < this._size.y)
                    nodeVisitor.VisitNode(node.north);

                if (coords.y - 1 >= 0)
                    nodeVisitor.VisitNode(node.south);
            }
        }

        private readonly System.Random _random;

        private readonly Container _container;

        public PathsGenerator(Container container, int seed)
        {
            this._container = container;
            this._random = new System.Random(seed);

        }

        public void Handle(World world)
        {
            var tileCovers = new TileCover[] {
                this._container.Get<TileCover>("tilecover.mud"),
                this._container.Get<TileCover>("tilecover.stone"),
            };

            for(var i = 0; i < 30; i ++)
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

                var navigator = new TerrainNavigator(world);
                var pathfinder = new AStarPathfinder<Tile>(navigator);
                var list = pathfinder.Pathfind(start, end);
                if (list == null) 
                {
                    continue;
                }

                var tilecover = tileCovers[this._random.Next(tileCovers.Length)];
                foreach (var tile in list)
                {
                    tile.SetTileCoverData(new TileCoverData(tilecover));                    
                    tile.baseCell.SetNavgridCost(tile.baseCell.navgridCost * 0.75f);
                }

            }
        }
    }
}