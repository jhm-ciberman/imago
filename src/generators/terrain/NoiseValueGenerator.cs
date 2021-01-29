using System;

namespace LifeSim.Generation
{
    public class NoiseValueGenerator : IValueGenerator
    {
        public struct Settings
        {
            public float scale;
            public float quality;
            public float gain;
            public float lacunarity;
        }

        private readonly FastNoiseLite _sampler;

        public NoiseValueGenerator(int seed, Settings settings)
        {
            float noiseToOctavesRatio = 0.5f;
            int octaves = System.Math.Max(1, (int) MathF.Ceiling(MathF.Sqrt(settings.scale) * noiseToOctavesRatio * settings.quality));

            this._sampler = new FastNoiseLite(seed);
            this._sampler.SetFractalType(FastNoiseLite.FractalType.FBm);
            this._sampler.SetFrequency(1f / settings.scale);
            this._sampler.SetFractalOctaves(octaves);
            this._sampler.SetFractalLacunarity(settings.lacunarity); 
            this._sampler.SetFractalGain(settings.gain); // Persistence
        }

        public float CalculateHeight(int x, int y)
        {
            return (this._sampler.GetNoise(x, y) + 1f) * 0.5f;
        }
    }
}