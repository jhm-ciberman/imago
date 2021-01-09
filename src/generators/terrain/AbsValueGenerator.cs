using System;

namespace LifeSim.Generation
{
    class AbsValueGenerator : IValueGenerator 
    {
        private IValueGenerator _generator;

        private float _scale;

        private float _offset;

        public AbsValueGenerator(IValueGenerator generator, float scale = 1f, float offset = 0f) 
        {
            this._generator = generator;
            this._scale = scale;
            this._offset = offset;
        }

        public float CalculateHeight(int x, int y) 
        {
            var value = this._generator.CalculateHeight(x, y) * 2f - 1f;

            var absValue = 1f - MathF.Abs(value * this._scale - this._offset);
            var sqrValue = (absValue * absValue);
            return 1f - sqrValue;
        }

    }

}