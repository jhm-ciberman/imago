using System;
using System.Numerics;
using LifeSim.Simulation;

namespace LifeSim.Generation
{
    public class WorldSettings
    {
        public Vector2Int size;
        public int seed;

        public float floraDensity = 0.3f;

        public bool generateHouses = true;

        public float elevationAmount = 0.36f;

        public float maximumWaterPercentage = 0.6f;

        public FalloffValueGenerator.Settings[] falloffs = new FalloffValueGenerator.Settings[] {
            new FalloffValueGenerator.Settings {
                minHeight = 0f,
                maxHeight = 1f,
                center = new Vector2(0.5f, 0.5f),
                size = new Vector2(0.9f, 0.9f),
                model = FalloffValueGenerator.FalloffModel.Circular,
                squareCoords = true,
                squareHeight = true,
            }
        };

        public bool noiseEnable = true;

        public NoiseValueGenerator.Settings noise = new NoiseValueGenerator.Settings {
            scale = 200f,
            quality = 0.7f,
            lacunarity = 2.0f,
            gain = 0.5f,
        };

        public NoiseValueGenerator.Settings riverNoise = new NoiseValueGenerator.Settings {
            scale = 200f,
            quality = 0.7f,
            lacunarity = 2.0f,
            gain = 0.5f,
        };

        public WorldSettings() : this(new Vector2Int(300, 300), 0) { }

        public WorldSettings(Vector2Int size, int seed)
        {
            if (seed == 0) {
                seed = new Random().Next();
            }
            this.seed = seed;
            this.size = size;
        }

        public WorldGenerator BuildGenerator()
        {
            Container container = new Container();
            EntitiesProvider entitiesProvider = new EntitiesProvider();
            entitiesProvider.Register(container);

            var valueGenerator = this._MakeValueGenerator();
            var landPercent = 1f - this.maximumWaterPercentage;
            float elevationAmount = this.elevationAmount * (0.5f + 0.5f * landPercent);
            
            var pipeline = new WorldGenerator(this.size);
            pipeline.Add(new TerrainGenerator(elevationAmount, valueGenerator));
            pipeline.Add(new WaterLevelCalculator(this.maximumWaterPercentage));
            pipeline.Add(new BiomeGenerator(container, this.seed));
            pipeline.Add(new FertilityGenerator(this.seed));
            if (this.floraDensity > 0f) {
                pipeline.Add(new FloraGenerator(container, this.seed, this.floraDensity));
            }
            if (this.generateHouses) { 
                pipeline.Add(new VillageGenerator(container, this.seed));
            }
            pipeline.Add(new NavgridGenerator());
            pipeline.Add(new TileCoverGenerator());
            pipeline.Add(new PathsGenerator(container, this.seed));
            pipeline.Add(new PlayerSpawner(this.seed));
            return pipeline;
        }

        private IValueGenerator _MakeValueGenerator() 
        {
            MergeValueGenerator generator = new MergeValueGenerator();

            if (this.noiseEnable) 
                generator.Add(new NoiseValueGenerator(this.seed, this.noise));

            //var rivers = new NoiseValueGenerator(this.seed + 1, this.riverNoise);
            //generator.Add(new AbsValueGenerator(rivers, 0.8f));

            foreach (var falloffSettings in this.falloffs)
            {
                generator.Add(new FalloffValueGenerator(this.size, falloffSettings));
            }

            return generator;
        }


    }
}