using System;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class FertilityGenerator : IWorldGenerationStep
    {
        private FastNoiseLite _noise;

        public FertilityGenerator(int seed)
        {
            // seed + 1 is to prevent generating a noise similar to the terrain height noise
            this._noise = new FastNoiseLite(seed + 1);
            this._noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        }

        public void Handle(World world)
        {
            foreach (Tile tile in world.tiles) {
                if (tile.height0 < world.waterLevel) {
                    tile.SetFertility(0);
                } else {
                    float value = 0.5f + this._noise.GetNoise(tile.coords.x, tile.coords.y) / 2f;
                    float fertility = tile.biome.fertilityMin + (tile.biome.fertilityMax - tile.biome.fertilityMin) * value;
                    tile.SetFertility(fertility);
                }
            }            
        }
    }
}