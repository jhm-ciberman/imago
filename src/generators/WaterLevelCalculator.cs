using System;
using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class WaterLevelCalculator : IWorldGenerationStep
    {
        private readonly float _maximumWaterPercentage;

        public WaterLevelCalculator(float maximumWaterPercentage)
        {
            this._maximumWaterPercentage = maximumWaterPercentage;
        }

        public void Handle(World world)
        {
            world.waterLevel = this._GetHeightPercentile(world, this._maximumWaterPercentage) - 0.25f;
            
            int[,] waterDistanceField = DistanceFieldCalculator.ComputeDistanceField(world, (terr, x, y) => {
                return (terr.GetTileAt(x, y).minHeight < terr.waterLevel);
            });

            foreach(Tile tile in world.tiles)
            {
                var coord = tile.coords;
                tile.SetDistanceToWater(waterDistanceField[coord.x, coord.y]);
                if (! tile.isAboveWater) tile.SetOcupation(Ocupation.Full);
            }
        }

        private float _GetHeightPercentile(World world, float percent)
        {
            List<float> heights = new List<float>(world.size.x * world.size.y);

            for (int x = 0; x < world.size.x; x++)
            {
                for (int y = 0; y < world.size.y; y++)
                {
                    heights.Add(world.GetTileAt(x, y).height0);
                }
            }

            heights.Sort();

            return heights[(int) MathF.Round((heights.Count - 1) * percent)];
        }


    }
}