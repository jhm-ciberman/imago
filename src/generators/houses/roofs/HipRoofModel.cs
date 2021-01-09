using System;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    class HipRoofModel : BaseRoofModel 
    {
        public override float GetRoofTileHeightValue(Roof roof)
        {
            var data = new RoofHeightMapKernel(roof);
            return 1f + this._GetNeighbourMinValue(data, data.value - 1f);
        }

        public override float[] GetRoofVertexHeights(Roof roof)
        {
            var roofHeights = new RoofHeightMapKernel(roof);
            float center      = roofHeights.value;
            float topLeft     = this._CornerHeight(roofHeights, new Vector2Int(-1, -1));
            float topRight    = this._CornerHeight(roofHeights, new Vector2Int(+1, -1));
            float bottomLeft  = this._CornerHeight(roofHeights, new Vector2Int(-1, +1));
            float bottomRight = this._CornerHeight(roofHeights, new Vector2Int(+1, +1));

            float top    = this._LateralGetHeight(roofHeights, new Vector2Int(0, -1), topLeft   , topRight   );
            float bottom = this._LateralGetHeight(roofHeights, new Vector2Int(0, +1), bottomLeft, bottomRight);
            float left   = this._LateralGetHeight(roofHeights, new Vector2Int(-1, 0), topLeft   , bottomLeft );
            float right  = this._LateralGetHeight(roofHeights, new Vector2Int(+1, 0), topRight  , bottomRight);

            return new float[] {
                topLeft       + 0.5f, top       + 0.5f, topRight      + 0.5f,
                left          + 0.5f, center    + 0.5f, right         + 0.5f,
                bottomLeft    + 0.5f, bottom    + 0.5f, bottomRight   + 0.5f,
            };
        }

        protected virtual float _LateralGetHeight(ISafeKernel<float> roofHeights, Vector2Int dir, float cornerHeight1, float cornerHeight2)
        {
            float center = roofHeights.value;
            float mean = (cornerHeight1 + cornerHeight2) / 2f;
            float lateral = roofHeights.SafeGet(dir.x, dir.y, center - 1f);
            float clampedLateral = Math.Clamp(lateral, mean - 0.5f, mean + 0.5f);
            if (lateral != clampedLateral) return mean;

            return (mean < lateral) ? (center + lateral) / 2 : mean;
        }

        protected virtual float _CornerHeight(ISafeKernel<float> roofHeights, Vector2Int dir)
        {
            float center = roofHeights.value;
            float a      = roofHeights.SafeGet(    0, dir.y, center - 1f);
            float b      = roofHeights.SafeGet(dir.x,     0, center - 1f);
            float corner = roofHeights.SafeGet(dir.x, dir.y, center - 1f);

            float mean = (a + b + corner) / 3f;

            if (mean == center) 
            {
                return corner + 0.5f;
            } 
            else 
            {
                float m = (mean < center) ? MathF.Min(Math.Min(a, b), corner) : Math.Max(Math.Max(a, b), corner);
                return Math.Clamp((m + center) / 2f, center - 0.5f, center + 0.5f);
            }
        }

    }
}