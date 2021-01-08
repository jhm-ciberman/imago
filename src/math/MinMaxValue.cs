namespace LifeSim
{
    public class MinMaxValue
    {
        public float min {get; private set;} = float.PositiveInfinity;
        public float max {get; private set;} = float.NegativeInfinity;

        public float delta => this.max - this.min;
        
        public float Value(float v)
        {
            max = v > max ? v : max;
            min = v < min ? v : min;
            return v;
        }

        public override string ToString()
        {
            return "Min: " + this.min + "\nMax: " + this.max;
        }
    }
}