
using System.Collections.Generic;

namespace LifeSim 
{
    internal class MergeValueGenerator : IValueGenerator 
    {

        private readonly List<IValueGenerator> _generators = new List<IValueGenerator>();

        public MergeValueGenerator() 
        {
            //
        }

        public void Add(IValueGenerator generator)
        {
            this._generators.Add(generator);
        }

        public float CalculateHeight(int x, int y) 
        {
            float value = 1;
            foreach (IValueGenerator generator in this._generators) 
            {
                value *= generator.CalculateHeight(x, y);
            }
            
            return value;
        }

    }

}