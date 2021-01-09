using System;
using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class HouseLotFinder 
    {
        public int minArea = 16;

        public int maxArea = 300;

        public float rectangleLikeFactor = 0.2f;

        public int minSideSize = 2;

        public int padding = 1;
        
        public List<HouseLot> Find(IEnumerable<FlatSurface> surfaces)
        {

            if (this.maxArea < this.minArea) 
            {
                throw new Exception("Max area cannot be less than min area");
            }

            if (this.minSideSize * this.minSideSize > this.maxArea) 
            {
                throw new Exception("The min side size squared is greater than the max permited area");
            }

            List<HouseLot> lots = new List<HouseLot>();

            foreach (FlatSurface surface in surfaces) 
            {
                while (true) 
                {
                    List<HouseLot> houseLots = this._CreateValidHouseLot(surface);
                    
                    if (houseLots.Count == 0) break;
                    lots.AddRange(houseLots);
                }
            }
            return lots;
        }

        private List<HouseLot> _CreateValidHouseLot(FlatSurface surface)
        {
            RectInt maxRect = new RectInt(new Vector2Int(0, 0), surface.size);
            return this._CreateValidHouseLot(surface, maxRect);
        }

        private List<HouseLot> _CreateValidHouseLot(FlatSurface surface, RectInt maxRect)
        {
            if (maxRect.size.x == 0 || maxRect.size.y == 0) 
            {
                return new List<HouseLot>() { };
            }
            
            RectInt lotRect = this._FindMaximumFittingSquare(surface, maxRect);
            PotentialHouseLot factory = new PotentialHouseLot(surface, lotRect, maxRect);

            if (factory.size.x < this.minSideSize)
            {
                return new List<HouseLot>() { };
            }
            
            factory.ExpandRect(this.rectangleLikeFactor);
            
            int area = factory.CalculateArea();

            if (area < this.minArea) 
            {
                return new List<HouseLot>() { };
            }
            
            if (area > this.maxArea) 
            {
                if (area / 2 >= this.minArea) 
                {
                    return this._SubdivideLot(factory);
                } 
                else 
                {
                    return new List<HouseLot> {this._ShrinkLot(factory)};
                }
            }

            factory.AddPadding(this.padding);

            return new List<HouseLot> { factory.CreateHouseLot() };
        }

        private HouseLot _ShrinkLot(PotentialHouseLot factory)
        {
            factory.ShrinkRect(this.maxArea);
            factory.AddPadding(this.padding);
            return factory.CreateHouseLot();
        }

        private List<HouseLot> _SubdivideLot(PotentialHouseLot factory)
        {
            RectInt totalRect = new RectInt(factory.start, factory.size);
            RectInt[] subdivisionRectangles = this._CreateSubdivisionRectangles(totalRect, this.padding);

            List<HouseLot> validLots = new List<HouseLot>();

            foreach (RectInt rect in subdivisionRectangles) 
            {
                List<HouseLot> lots = this._CreateValidHouseLot(factory.surface, rect);
                if (lots.Count == 0) 
                {
                    return new List<HouseLot> { this._ShrinkLot(factory) };
                } 
                else
                {
                    validLots.AddRange(lots);
                }
            }

            return validLots;
        }

        
        private RectInt[] _CreateSubdivisionRectangles(RectInt rect, int padding) 
        {
            int p = padding;
            Vector2Int start = rect.min;
            Vector2Int size = rect.size;

            RectInt rectA;
            RectInt rectB;

            if (size.x > size.y) 
            {
                // Vertical subdivision
                int width = ~~((size.x - p) / 2);
                rectA = new RectInt(new Vector2Int(start.x, start.y), new Vector2Int(width, size.y));
                rectB = new RectInt(new Vector2Int(start.x + width + p, start.y), new Vector2Int(size.x - width - p, size.y));
            } 
            else 
            {
                // horizontal subdivision
                int height = ~~((size.y - p) / 2);
                rectA = new RectInt(new Vector2Int(start.x, start.y), new Vector2Int(size.x, height));
                rectB = new RectInt(new Vector2Int(start.x, start.y + height + p), new Vector2Int(size.x, size.y - height - p));
            }

            return new RectInt[] {rectA, rectB};
        }


        public RectInt _FindMaximumFittingSquare(FlatSurface surf)
        {
            return this._FindMaximumFittingSquare(surf, new RectInt(new Vector2Int(0, 0), surf.size));
        }

        public RectInt _FindMaximumFittingSquare(FlatSurface surf, RectInt area)
        {
            int maxValue = 0;
            Vector2Int maxValueCoord = new Vector2Int(0, 0);
            Grid<int> areasGrid = new Grid<int>(area.size);

            for (int y = 0; y < area.size.y; y++)
            {
                areasGrid.Set(0, y, surf.Get(area.min.x, area.min.y + y) ? 1 : 0);
            }
            
            for (int x = 1; x < area.size.x; x++)
            {
                areasGrid.Set(x, 0, surf.Get(area.min.x + x, area.min.y) ? 1 : 0);

                for (int y = 1; y < area.size.y; y++)
                {
                    if (! surf.Get(area.min.x + x, area.min.y + y)) continue; 

                    int a = areasGrid.Get(x    , y - 1); 
                    int b = areasGrid.Get(x - 1, y    );
                    int c = areasGrid.Get(x - 1, y - 1);

                    int value = 1 + Math.Min(a, Math.Min(b, c));
                    areasGrid.Set(x, y, value);

                    if (value > maxValue)
                    {
                        maxValue = value;
                        maxValueCoord = new Vector2Int(x, y);
                    }
                }         
            }

            Vector2Int start = new Vector2Int(area.min.x + maxValueCoord.x - maxValue + 1, area.min.y + maxValueCoord.y - maxValue + 1);
            Vector2Int size = new Vector2Int(maxValue, maxValue);
            return new RectInt(start, size);
        }

    }
}