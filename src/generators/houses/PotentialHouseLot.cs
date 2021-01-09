using System;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    class PotentialHouseLot 
    {

        public Vector2Int start;
        public Vector2Int size;
        public readonly FlatSurface surface;

        private RectInt _maxRect;

        public PotentialHouseLot(FlatSurface surface, RectInt rect, RectInt maxRect) 
        {
            this.surface = surface;
            this._maxRect = maxRect;
            this.start = rect.min;
            this.size = rect.size;
        }

        private int _GetColumnArea(int x)
        {
            if (! this.surface.IsInsideBounds(x, this.start.y)) return 0;

            int yEnd = this.start.y + this.size.y;
            int area = 0;
            for (int y = this.start.y; y < yEnd; y++)
            {
                if (this.surface.Get(x, y))
                {
                    area += 1;
                }
            }
            return area;
        }

        private int _GetRowArea(int y)
        {
            if (! this.surface.IsInsideBounds(this.start.x, y)) return 0;

            int xEnd = this.start.x + this.size.x;
            int area = 0;
            for (int x = this.start.x; x < xEnd; x++)
            {
                if (this.surface.Get(x, y))
                {
                    area += 1;
                }
            }
            return area;
        }

        public int CalculateArea()
        {
            int area = 0;
            for (int x = 0; x < this.size.x; x++)
            {
                for (int y = 0; y < this.size.y; y++)
                {
                    if (this.surface.Get(this.start.x + x, this.start.y + y))
                    {
                        area += 1;
                    }
                }
            }
            return area;
        }

        public bool IsInsideRect(int x, int y)
        {
            return (x >= this.start.x && y >= this.start.y && x < this.start.x + this.size.x && y < this.start.y + this.size.y);
        }

        public string GetDebugString()
        {
            string str = "";
            for (int y = 0; y < this.surface.size.y; y++)
            {
                for (int x = 0; x < this.surface.size.x; x++)
                {
                    if (this.surface.Get(x, y))
                    {
                        str += this.IsInsideRect(x, y) ? ". " : "  ";
                    } 
                    else
                    {
                        str += this.IsInsideRect(x, y) ? "@ " : "X ";
                    }
                }
                str += "\n";
            }
            return str;
        }

        public void ShrinkRect(int maxArea)
        {
            int area = this.CalculateArea();
            
            while(area > maxArea)
            {
                int areaLeft   = this._GetColumnArea(this.start.x);
                int areaRight  = this._GetColumnArea(this.start.x + this.size.x - 1);
                int areaTop    = this._GetRowArea(this.start.y);
                int areaBottom = this._GetRowArea(this.start.y + this.size.y - 1);

                int value = Math.Min(Math.Min(areaLeft, areaRight), Math.Min(areaTop, areaBottom));

                if (value == areaLeft)
                {
                    this.start.x += 1; 
                    this.size.x -= 1;
                    area -= areaLeft;
                } 
                else if (value == areaRight)
                {
                    this.size.x -= 1; 
                    area -= areaRight;  
                } 
                else if (value == areaTop)
                {
                    this.start.y += 1; 
                    this.size.y -= 1;  
                    area -= areaTop;  
                }
                else if (value == areaBottom) 
                {
                    this.size.y -= 1;
                    area -= areaBottom; 
                } 
                else 
                {
                    throw new Exception("Errorrrrr");
                }

            }
        }

        public void ExpandRect(float rectangleLikeRatio)
        {
            //rectangleLikeRatio = Math.max(0.01, rectangleLikeRatio);
            int x0 = this._maxRect.min.x;
            int y0 = this._maxRect.min.y;
            int x1 = this._maxRect.max.x;
            int y1 = this._maxRect.max.y;
            
            for (;;)
            {
                float areaLeft   = (this.start.x - 1 < x0)            ? 0 : this._GetColumnArea(this.start.x - 1)           / this.size.y;
                float areaRight  = (this.start.x + this.size.x >= x1) ? 0 : this._GetColumnArea(this.start.x + this.size.x) / this.size.y;
                float areaTop    = (this.start.y - 1 < y0)            ? 0 : this._GetRowArea(this.start.y - 1)              / this.size.x;
                float areaBottom = (this.start.y + this.size.y >= y1) ? 0 : this._GetRowArea(this.start.y + this.size.y)    / this.size.x;

                float value = MathF.Max(MathF.Max(areaLeft, areaRight), MathF.Max(areaTop, areaBottom));

                if (value <= rectangleLikeRatio) break;

                if (value == areaLeft)
                {
                    this.start.x -= 1; 
                    this.size.x += 1; 
                } 
                else if (value == areaRight)
                {
                    this.size.x += 1; 
                }
                else if (value == areaTop)
                {
                    this.start.y -= 1; 
                    this.size.y += 1;  
                }
                else if (value == areaBottom)
                {
                    this.size.y += 1; 
                }
            }
        }

        public HouseLot CreateHouseLot()
        {
            Vector2Int surfCoords = this.surface.coords;
            
            Grid<bool> lotCells = new Grid<bool>(this.size);

            for (int x = 0; x < this.size.x; x++)
            {
                for (int y = 0; y < this.size.y; y++)
                {
                    bool value = this.surface.Get(this.start.x + x, this.start.y + y);
                    lotCells.Set(x, y, value);
                    this.surface.RemoveTile(this.start.x + x, this.start.y + y);
                }
            }
            
            Vector2Int coords = new Vector2Int(surfCoords.x + this.start.x, surfCoords.y + this.start.y);
            return new HouseLot(this.surface.world, coords, lotCells);
        }


        private void _RemoveTile(int x, int y) 
        {
            if (this.surface.IsInsideBounds(x, y))
            {
                this.surface.RemoveTile(x, y);
            }
        }

        public void AddPadding(int padding)
        {
            int xEnd = this.start.x + this.size.x;
            int yEnd = this.start.y + this.size.y;

            for (int p = 0; p < padding; p++)
            {
                for (int x = this.start.x - 1; x <= xEnd; x++)
                {
                    this._RemoveTile(x, this.start.y - p - 1);
                    this._RemoveTile(x, yEnd + p);
                }
        
                for (int y = this.start.y - 1; y <= yEnd; y++)
                {
                    this._RemoveTile(this.start.x - p - 1, y);
                    this._RemoveTile(xEnd + p, y);
                }
            }
        }
    }
}