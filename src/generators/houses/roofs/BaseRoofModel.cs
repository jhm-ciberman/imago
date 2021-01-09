using System;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public abstract class BaseRoofModel : IRoofModel
    {
        protected readonly struct RoofHeightMapKernel : ISafeKernel<float>
        {
            private readonly Roof _roof;
            private readonly Vector2Int _center;

            public RoofHeightMapKernel(Roof roof)
            {
                this._roof = roof;
                this._center = roof.coords;
            }

            public float value => this._roof.roofHeight;

            public float SafeGet(int x, int y, float defaultValue) 
            {
                var c = this._center + new Vector2Int(x, y);
                var world = this._roof.tile.world;

                if (! world.TileCoordIsInside(c.x, c.y)) return defaultValue;

                var r = world.GetTileAt(c).roof;
                return r.hasRoof ? r.roofHeight : defaultValue;
            }
        }

        public abstract float GetRoofTileHeightValue(Roof roof);

        public abstract float[] GetRoofVertexHeights(Roof roof);

        protected float _GetNeighbourMinValue(ISafeKernel<float> data, float defaultValue)
        {
            var values = new [] {
                data.SafeGet(-1, -1, defaultValue),
                data.SafeGet( 0, -1, defaultValue),
                data.SafeGet(+1, -1, defaultValue),

                data.SafeGet(-1,  0, defaultValue),
                data.SafeGet(+1,  0, defaultValue),

                data.SafeGet(-1, +1, defaultValue),
                data.SafeGet( 0, +1, defaultValue),
                data.SafeGet(+1, +1, defaultValue)
            };

            float m = values[0];
            for (int i = 1; i < values.Length; i++)
                if (values[i] < m) m = values[i];

            return m;
        }
    }
}