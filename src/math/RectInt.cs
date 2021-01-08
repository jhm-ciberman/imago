using System;

namespace LifeSim
{
    public struct RectInt
    {
        private Vector2Int coords;
        private Vector2Int size;

        public RectInt(Vector2Int coords, Vector2Int size)
        {
            this.coords = coords;
            this.size = size;
        }

        public Vector2Int min => new Vector2Int(this.xMin, this.yMin);
        public Vector2Int max => new Vector2Int(this.xMax, this.yMax);

        public int xMin => Math.Min(this.coords.x, this.coords.x + this.size.x);
        public int yMin => Math.Min(this.coords.y, this.coords.y + this.size.y);
        public int xMax => Math.Max(this.coords.x, this.coords.x + this.size.x);
        public int yMax => Math.Max(this.coords.y, this.coords.y + this.size.y);

        public bool Contains(Vector2Int position)
        {
            return position.x >= this.xMin
                && position.y >= this.yMin
                && position.x < this.xMax
                && position.y < this.yMax;
        }

        public bool Overlaps(RectInt other)
        {
            return other.xMin < this.xMax
                && other.xMax > this.xMin
                && other.yMin < this.yMax
                && other.yMax > this.yMin;
        }
    }
}