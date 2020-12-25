using System.Numerics;

namespace LifeSim.Rendering
{
    public class Animation
    {
        public class Channel
        {

        }

        public class SamplerLinear<T> where T : struct
        {
            public float[] times;
            public T[] values;

            public SamplerLinear(float[] times, T[] values)
            {
                this.times = times;
                this.values = values;
            }

            //public Vector3 Sample(float time)
            //{
            //    
            //    return this.values[];
            //}
        }

        private string _name;

        public Animation(string name)
        {
            this._name = name;
        }
    }
}