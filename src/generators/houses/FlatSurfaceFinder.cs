using System;
using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    class FlatSurfaceFinder 
    {
        private World _world;

        private Grid<bool> _visitedTiles;

        private RectInt _currentBox = new RectInt();

        private List<Vector2Int> _currentTilesCoordsList = new List<Vector2Int>();

        private float _currentHeight = 0;

        private List<FlatSurface> _surfaces = new List<FlatSurface>();

        private Stack<Vector2Int> _stack = new Stack<Vector2Int>(100);

        public List<FlatSurface> surfaces => this._surfaces;

        public FlatSurfaceFinder(World world) 
        {
            this._world = world;
            this._visitedTiles = new Grid<bool>(this._world.size);

            int minArea = 10;

            for (int x = 0; x < this._world.size.x; x++) 
            {
                for (int y = 0; y < this._world.size.y; y++) 
                {
                    this._FloodFill(new Vector2Int(x, y));

                    if (this._currentTilesCoordsList.Count >= minArea) 
                    {
                        this._AddSurface();
                    }
                }
            }
        }

        private void _FloodFill(Vector2Int startingCoord) 
        {
            if (this._visitedTiles.Get(startingCoord)) return;
            Tile tile = this._world.GetTileAt(startingCoord);
            if (! tile.isEmpty) return;
            if (! tile.isFlat) return;

            this._currentTilesCoordsList.Clear();
            this._currentBox.x = startingCoord.x;
            this._currentBox.y = startingCoord.y;
            this._currentBox.width = 0;
            this._currentBox.height = 0;
            this._currentHeight = tile.height0;
            this._stack.Clear();
            
            this._MarkTileAsVisited(startingCoord);

            while (this._stack.Count > 0)
            {
                Vector2Int coord = this._stack.Pop();

                if (this._TileIsValid(coord))
                {
                    this._MarkTileAsVisited(coord);
                }
            }
        }

        private bool _TileIsValid(Vector2Int coord) 
        {
            if (this._visitedTiles.SafeGet(coord, true)) return false;
            Tile tile = this._world.GetTileAt(coord);
            if (tile.height0 != this._currentHeight) return false;
            if (! tile.isEmpty) return false;
            if (! tile.isFlat) return false;
            return true;
        }

        private void _AddSurface() 
        {
            Vector2Int coords = this._currentBox.min;
            Vector2Int size = this._currentBox.size + new Vector2Int(1, 1);
            FlatSurface surface = new FlatSurface(this._world, coords, size, this._currentHeight);

            foreach (Vector2Int tile in this._currentTilesCoordsList) 
            {
                surface.AddTile(tile.x - coords.x, tile.y - coords.y);
            }

            this._surfaces.Add(surface);
            this._currentTilesCoordsList.Clear();
        }

        private void _MarkTileAsVisited(Vector2Int coord) 
        {
            this._visitedTiles.Set(coord, true);
            this._ExpandByPoint(coord);
            this._currentTilesCoordsList.Add(coord);

            // Use Flood fill algorithm to visit all tiles
            this._AddCoordToStack(coord + new Vector2Int(+ 1,   0));
            this._AddCoordToStack(coord + new Vector2Int(  0, + 1));
            this._AddCoordToStack(coord + new Vector2Int(- 1,   0));
            this._AddCoordToStack(coord + new Vector2Int(  0, - 1));
        }

        private void _AddCoordToStack(Vector2Int coord)
        {
            if (! this._visitedTiles.SafeGet(coord, true))
            {
                this._stack.Push(coord);
            }
        }

        private void _ExpandByPoint(Vector2Int point)
        {
            RectInt box = this._currentBox;
            this._currentBox.xMin = Math.Min( box.xMin, point.x );
            this._currentBox.yMin = Math.Min( box.yMin, point.y );
            this._currentBox.xMax = Math.Max( box.xMax, point.x );
            this._currentBox.yMax = Math.Max( box.yMax, point.y );
        }


    }
}