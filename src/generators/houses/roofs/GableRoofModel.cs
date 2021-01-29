using System;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    internal class GableRoofModel : HipRoofModel 
    {
        protected Vector2Int _dir;

        public GableRoofModel(bool horizontal = true)
        {
            this._dir = horizontal ? new Vector2Int(0, 1) : new Vector2Int(1, 0);
        }

        public override float GetRoofTileHeightValue(Roof roof)
        {
            var data = new RoofHeightMapKernel(roof);
            float center = data.value - 1f;

            return 1f + MathF.Min(
                data.SafeGet(-this._dir.x,-this._dir.y, center),
                data.SafeGet( this._dir.x, this._dir.y, center)
            );
        }

        protected override float _LateralGetHeight(ISafeKernel<float> roofHeights, Vector2Int dir, float cornetHeight1, float cornerHeight2)
        {
            if (this._dir.y != 0 && dir.x != 0) return roofHeights.value;
            if (this._dir.x != 0 && dir.y != 0) return roofHeights.value;

            return (cornetHeight1 + cornerHeight2) / 2;
        }

        protected override float _CornerHeight(ISafeKernel<float> roofHeights, Vector2Int dir)
        {
            var d = dir * this._dir;
            float center = roofHeights.value;
            float corner = roofHeights.SafeGet(d.x, d.y, center - 1f);

            float mean = Math.Clamp((center + corner) / 2, center - 0.5f, center + 0.5f);

            return (mean == center) ? center + 0.5f : mean;
        }

    }
}