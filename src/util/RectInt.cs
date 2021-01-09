using System;

namespace LifeSim
{
    public struct RectInt
    {
        public Vector2Int coords;
        public Vector2Int size;

        public RectInt(Vector2Int coords, Vector2Int size)
        {
            this.coords = coords;
            this.size = size;
        }

        public Vector2Int min => new Vector2Int(this.xMin, this.yMin);
        public Vector2Int max => new Vector2Int(this.xMax, this.yMax);

        public int xMin { get => Math.Min(this.coords.x, this.coords.x + this.size.x); set { int oldxmax = this.xMax; this.coords.x = value; this.size.x = oldxmax - this.coords.x; } }
        public int yMin { get => Math.Min(this.coords.y, this.coords.y + this.size.y); set { int oldymax = this.yMax; this.coords.y = value; this.size.y = oldymax - this.coords.y; } }
        public int xMax { get => Math.Max(this.coords.x, this.coords.x + this.size.x); set { this.size.x = value - this.coords.x; } }
        public int yMax { get => Math.Max(this.coords.y, this.coords.y + this.size.y); set { this.size.y = value - this.coords.y; } }

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