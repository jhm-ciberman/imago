using System;
using System.Collections.Generic;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class BiomeGenerator : IWorldGenerationStep
    {
        public struct BiomeHeightInfo
        {
            public float topHeight;
            public Biome biome;
        }

        private FastNoiseLite _noise;
        private int _seed;
        private IContainer _container; 

        public BiomeGenerator(IContainer container, int seed)
        {
            this._container = container;
            this._seed = seed;
            //this._random = new System.Random(seed);
            this._noise = new FastNoiseLite(seed + 123);
            this._noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            this._noise.SetFractalOctaves(2);
        }

        public void Handle(World world)
        {
            var sand = this._container.Get<Biome>("biome.sand");
            var grass = this._container.Get<Biome>("biome.grass");
            var regions = this._MakeRegions(world);
            foreach (Tile tile in world.tiles)
            {
                var hDif = (tile.avgHeight - world.minMaxHeight.min) / 4f;
                var w = (tile.distanceToWater / 100f);
                
                var coords = tile.coords;
                var d = (this._noise.GetNoise(coords.x, coords.y) + 1f) / 2f;
                
                var biome = (w < 1f * d) ? sand : grass;

                tile.SetBiome(biome);
                tile.SetTileCoverData(new TileCoverData(this._ResolveTilecover(regions, tile.biome)));
            }
        }

        protected float _CalculateBiomeTopHeight(World world, float topHeightPercent)
        {
            var minHeight = MathF.Floor(world.waterLevel);
            var delta = MathF.Ceiling(world.minMaxHeight.max) - minHeight;
            return minHeight + MathF.Round(topHeightPercent * delta / Tile.snapIncrement) * Tile.snapIncrement + Tile.snapIncrement / 2f;
        }

        private Biome _GetBiomeForHeight(BiomeHeightInfo[] regions, float height)
        {
            foreach (var region in regions)
            {
                if (height < region.topHeight) return region.biome;
            }

            return regions[regions.Length - 1].biome;
        }


        private Dictionary<Biome, TileCover> _MakeRegions(World world)
        {
            return new Dictionary<Biome, TileCover> {
                { this._container.Get<Biome>("biome.sand"),  this._container.Get<TileCover>("tilecover.sand")  },
                { this._container.Get<Biome>("biome.grass"), this._container.Get<TileCover>("tilecover.grass") },
            };
        }

        private TileCover _ResolveTilecover(Dictionary<Biome, TileCover> regions, Biome biome)
        {
            if (biome != null && regions.TryGetValue(biome, out TileCover? cover)) {
                return cover;
            }

            throw new Exception("Tilecover not found");
        }

    }

}