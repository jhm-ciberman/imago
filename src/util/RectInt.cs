using System;

namespace LifeSim
{
    public struct RectInt
    {
        public int x;
        public int y;
        public int width;
        public int height;

        public RectInt(Vector2Int coords, Vector2Int size)
        {
            this.x = coords.x;
            this.y = coords.y;
            this.width = size.x;
            this.height = size.y;
        }

        public RectInt(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public Vector2Int coords => new Vector2Int(this.x, this.y);
        public Vector2Int size => new Vector2Int(this.width, this.height);

        public Vector2Int min => new Vector2Int(this.xMin, this.yMin);
        public Vector2Int max => new Vector2Int(this.xMax, this.yMax);

        public int xMin { get => Math.Min(this.x, this.x + this.width ); set { int oldxmax = this.xMax; this.x = value; this.width = oldxmax - this.x; } }
        public int yMin { get => Math.Min(this.y, this.y + this.height); set { int oldymax = this.yMax; this.y = value; this.height = oldymax - this.y; } }
        public int xMax { get => Math.Max(this.x, this.x + this.width ); set { this.width = value - this.x; } }
        public int yMax { get => Math.Max(this.y, this.y + this.height); set { this.height = value - this.y; } }

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