using System;

namespace LifeSim
{
    public class RandomGauss
    {
        private Random _random;

        public RandomGauss() : this(new Random()) {}

        public RandomGauss(int seed) : this(new Random(seed)) {}

        public RandomGauss(System.Random random)
        {
            this._random = random;
        }

        private double _Generate()
        {
            double u, v, S;

            do
            {
                u = 2.0 * this._random.NextDouble() - 1.0;
                v = 2.0 * this._random.NextDouble() - 1.0;
                S = u * u + v * v;
            }
            while (S >= 1.0);

            double fac = Math.Sqrt(-2.0 * Math.Log(S) / S);
            return u * fac;
        }

        public double NextGaussian()
        {
            return this.NextGaussian(0, 1);
        }

        public double NextGaussian(double minValue, double maxValue)
        {
            double mean = (minValue + maxValue) / 2;
            double sigma = (maxValue - mean) / 3;
            double value = mean + this._Generate() * sigma;
            return Math.Min(maxValue, Math.Max(minValue, value));
        }
    }
}