using LifeSim.Simulation;

namespace LifeSim.Generation
{
    class FlatRoofModel : BaseRoofModel 
    {
        public override float GetRoofTileHeightValue(Roof roof)
        {
            var data = new RoofHeightMapKernel(roof);
            return this._GetNeighbourMinValue(data, data.value);
        }

        public override float[] GetRoofVertexHeights(Roof roof)
        {
            float height = roof.roofHeight;
              
            return new float[] { 
                height, height, height,
                height, height, height,
                height, height, height,
            };
        }
    }
}