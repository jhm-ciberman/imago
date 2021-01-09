using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class NavgridGenerator : IWorldGenerationStep
    {

        ///private float _distanceToWater = 15f;
        ///private float _distanceToWaterCost = 1.8f;

        private float _distanceToNonEmpty = 18f;
        private float _distanceToNonEmptyCost = 1.5f;

        private float _baseCost = 3f;


        public void Handle(World world)
        {
            var emptinessDistanceField = DistanceFieldCalculator.ComputeDistanceField(world, (terr, x, y) => {
                return ! terr.GetTileAt(x, y).isWalkable;
            });

            foreach (var tile in world.tiles)
            {
                float cost = float.PositiveInfinity;

                if (tile.isWalkable)
                {
                    cost = tile.biome.navgridCost * this._baseCost;
                    
                    /*
                    float distToWater = ((float) tile.distanceToWater / 10f) / this._distanceToWater;
                    if (distToWater < 1f) 
                    {
                        distToWater = (1f - distToWater);
                        cost *= Mathf.Lerp(1f, this._distanceToWaterCost, distToWater * distToWater);
                    }
                    */

                    var c = tile.coords;
                    float distToNonEmpty = (emptinessDistanceField[c.x, c.y] / 10f) / this._distanceToNonEmpty;
                    if (distToNonEmpty < 1f) 
                    {
                        distToNonEmpty = (1f - distToNonEmpty);
                        var t = distToNonEmpty * distToNonEmpty;
                        cost *= 1f + (this._distanceToNonEmptyCost - 1f) * t;
                    }
                    
                }

                tile.GetCellAtLevel(0).SetNavgridCost(cost);
            }
        }
    }
}