using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class FlatSurface 
    {
        
        private readonly bool[,] _cells;

        /**
        * The coordinate of the flat surface in the world, measured in tiles
        */
        public readonly Vector2Int coords;

        /**
        * The y coordinate of the flat surface
        */
        public readonly float height;

        private int _area = 0;

        public World world;

        public Vector2Int size;

        public FlatSurface(World world, Vector2Int coords, Vector2Int size, float height) 
        {
            this.world = world;
            this.coords = coords;
            this.height = height;
            this.size = size;
            this._cells = new bool[size.x, size.y];
        }

        public void AddTile(int x, int y) 
        {
            if (this._cells[x, y] == false) 
            {
                this._cells[x, y] = true;
                this._area += 1;
            }
        }

        public void RemoveTile(int x, int y) 
        {
            if (this._cells[x, y] == true) 
            {
                this._cells[x, y] = false;
                this._area -= 1;
            }
        }

        public int area => this._area;
        
        public bool Get(int x, int y) => this._cells[x, y];
        public bool IsInsideBounds(int x, int y) => (x >= 0 && y >= 0 && x < this.size.x && y < this.size.y);

    }
}
