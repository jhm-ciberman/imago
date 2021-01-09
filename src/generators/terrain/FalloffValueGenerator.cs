using System;
using System.Numerics;

namespace LifeSim.Generation
{
    public class FalloffValueGenerator: IValueGenerator 
    {
        [System.Serializable]
        public enum FalloffModel
        {
            Circular,
            Square,
        }

        [System.Serializable]
        public struct Settings
        {
            public float minHeight;
            public float maxHeight;
            public Vector2 center;   
            public Vector2 size;
            public FalloffModel model;
            public bool squareCoords;
            public bool squareHeight;
        }

        private GenerateValue _generatorFunction;

        private delegate float GenerateValue(Vector2 pos);

        public Vector2 center = new Vector2(0.5f, 0.5f);

        public Vector2 size = Vector2.One;

        public Vector2Int mapSize;

        public bool squareCoords = true;

        public bool squareValue = true;

        public bool invert = false;

        public float minValue = 0f;

        public float maxValue = 0f;

        private FalloffModel _model = FalloffModel.Circular;

        public FalloffValueGenerator(Vector2Int mapSize) 
        {
            this.mapSize = mapSize;
            this._generatorFunction = this._GenerateCircularFalloff;
        }

        public FalloffValueGenerator(Vector2Int mapSize, Settings settings) 
        {
            this.mapSize = mapSize;

            this._generatorFunction = this._GenerateCircularFalloff;
            this.falloffModel = settings.model;
            this.minValue = settings.minHeight;
            this.maxValue = settings.maxHeight;
            this.size = settings.size;
            this.center = settings.center;
            this.squareCoords = settings.squareCoords;
            this.squareValue = settings.squareHeight;
        }

        public FalloffModel falloffModel
        {
            get => this._model;
            set 
            {
                this._model = value;

                if (value == FalloffModel.Circular) {
                    this._generatorFunction = this._GenerateCircularFalloff;
                } else {
                    this._generatorFunction = this._GenerateSquareFalloff;
                }
            }
        }

        private float _GenerateCircularFalloff(Vector2 pos)
        {
            return pos.Length();
        }

        private float _GenerateSquareFalloff(Vector2 pos)
        {
            return MathF.Max(MathF.Abs(pos.X), MathF.Abs(pos.Y));
        }

        public float CalculateHeight(int x, int y)
        {
            Vector2 normalized = new Vector2((float) x / (float) this.mapSize.x, (float) y / (float) this.mapSize.y);
            normalized = (normalized - this.center) * (new Vector2(2f / this.size.X, 2f / this.size.Y));

            if (this.squareCoords) normalized *= normalized;

            float value = this._generatorFunction(normalized);

            if (this.squareValue) value *= value;

            value = this.minValue + (this.maxValue - this.minValue) * value;

            return 1f - value;
        }
    }
}