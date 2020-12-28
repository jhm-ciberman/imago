using System.Collections.Generic;
using System.Numerics;
using LifeSim.SceneGraph;

namespace LifeSim.Anim
{
    public class Animation
    {
        public interface IChannel
        {
            string targetName {get;}
            float duration {get;}
            void UpdateTarget(Node3D target, float time, bool loop, ref int lastTimeIndex);
        }

        public abstract class BaseChannel<T> : IChannel where T : struct
        {
            private ISampler<T> _sampler;
            public string targetName {get;}

            public float duration => this._sampler.duration;

            public BaseChannel(string targetName, ISampler<T> sampler)
            {
                this._sampler = sampler;
                this.targetName = targetName;
            }

            protected abstract void SetTargetValue(Node3D node, ref T value);

            public void UpdateTarget(Node3D target, float time, bool loop, ref int lastTimeIndex)
            {
                T value = this._sampler.Sample(time, loop, ref lastTimeIndex);
                this.SetTargetValue(target, ref value);
            }
        }

        public class PositionChannel : BaseChannel<Vector3>
        {
            public PositionChannel(string targetName, ISampler<Vector3> sampler) : base(targetName, sampler) { }
            protected override void SetTargetValue(Node3D node, ref Vector3 value) => node.position = value;
        }

        public class ScaleChannel : BaseChannel<Vector3>
        {
            public ScaleChannel(string targetName, ISampler<Vector3> sampler) : base(targetName, sampler) { }
            protected override void SetTargetValue(Node3D node, ref Vector3 value) => node.scale = value;
        }

        public class RotationChannel : BaseChannel<Quaternion>
        {
            public RotationChannel(string targetName, ISampler<Quaternion> sampler) : base(targetName, sampler) { }
            protected override void SetTargetValue(Node3D node, ref Quaternion value) => node.rotation = value;
        }

        public interface ISampler<T>
        {
            float duration { get; }
            T Sample(float time, bool loop, ref int lastTimeIndex);
        }

        public abstract class BaseSampler<T> : ISampler<T>
        {
            protected float[] _times;
            protected T[] _values;

            public float duration => this._times[this._times.Length - 1];

            public BaseSampler(float[] times, T[] values)
            {
                this._times = times;
                this._values = values;
            }

            protected abstract T _Interpolate(int indexPrev, int indexNext, float time);

            private int _FindNextIndex(float time, int lastTimeIndex) //TODO: optimize with lastTimeIndex
            {
                for (int i = 0; i < this._times.Length; i++) {
                    if (this._times[i] >= time) {
                        return i;
                    }
                }
                return this._times.Length - 1;
            }

            public T Sample(float time, bool loop, ref int lastTimeIndex)
            {
                var nextIndex = this._FindNextIndex(time, lastTimeIndex);
                lastTimeIndex = nextIndex;

                var prevIndex = loop 
                    ? (nextIndex - 1) % this._times.Length 
                    : System.Math.Min(nextIndex - 1, 0);

                return this._Interpolate(prevIndex, nextIndex, time);
            }
        }

        public class SamplerLinear<T> : BaseSampler<T>
        {
            protected IInterpolator<T> _interpolator;
            
            public SamplerLinear(float[] times, T[] values, IInterpolator<T> interpolator) : base(times, values)
            {
                this._interpolator = interpolator;
            }

            protected override T _Interpolate(int indexPrev, int indexNext, float time)
            {
                var prev = this._values[indexPrev];
                var next = this._values[indexNext];
                var tPrev = this._times[indexPrev];
                var tNext = this._times[indexNext];
                var t = (time - tPrev) / (tNext - tPrev);
                return this._interpolator.Linear(prev, next, t);
            }
        }

        public class SamplerStep<T> : BaseSampler<T>
        {
            public SamplerStep(float[] times, T[] values, IInterpolator<T> interpolator) : base(times, values) 
            {
                //
            }

            protected override T _Interpolate(int indexPrev, int indexNext, float time)
            {
                return this._values[indexPrev];
            }
        }

        public class SamplerCubicSpline<T> : BaseSampler<T>
        {
            protected IInterpolator<T> _interpolator;

            public SamplerCubicSpline(float[] times, T[] values, IInterpolator<T> interpolator) : base(times, values) 
            {
                this._interpolator = interpolator;
            }

            protected override T _Interpolate(int indexPrev, int indexNext, float time)
            {
                return this._values[indexPrev]; //TODO: Implement cubic spline
            }
        }

        public interface IInterpolator<T>
        {
            T Linear(T prev, T next, float t);
            T CubicSpline(T previousValue, T previousTangent, T nextValue, T nextTangent, float t);
        }

        public class Vector3Interpolator : IInterpolator<Vector3>
        {
            public Vector3 Linear(Vector3 prev, Vector3 next, float t)
            {
                return Vector3.Lerp(prev, next, t);
            }

            public Vector3 CubicSpline(Vector3 previousValue, Vector3 previousTangent, Vector3 nextValue, Vector3 nextTangent, float t)
            {
                var t2 = t * t;
                var t3 = t2 * t;
                return (2f * t3 - 3f * t2 + 1f) * previousValue 
                     + (t3 - 2f * t2 + t) * previousTangent 
                     + (-2f * t3 + 3f * t2) * nextValue 
                     + (t3 - t2) * nextTangent;
            }
        }

        public class QuaternionInterpolator : IInterpolator<Quaternion>
        {
            public Quaternion Linear(Quaternion prev, Quaternion next, float t)
            {
                return Quaternion.Slerp(prev, next, t);
            }

            public Quaternion CubicSpline(Quaternion previousValue, Quaternion previousTangent, Quaternion nextValue, Quaternion nextTangent, float t)
            {
                return Quaternion.Slerp(previousValue, nextValue, t); // Not implemented! 
            }
        }

        private Dictionary<string, List<IChannel>> _channels = new Dictionary<string, List<IChannel>>();
        private string _name;
        private float _duration;
        public float duration => this._duration;

        public Animation(string name, IReadOnlyList<IChannel> channels)
        {
            this._name = name;

            float duration = 0;
            foreach (var channel in channels) {
                duration = System.Math.Max(channel.duration, duration);

                string key = channel.targetName;
                if (this._channels.TryGetValue(key, out List<IChannel>? list)) {
                    list.Add(channel);
                } else {
                    this._channels.Add(channel.targetName, new List<IChannel> { channel });
                }
            }
            this._duration = duration;
        }

        public IReadOnlyList<IChannel>? FindChannels(string targetName)
        {
            this._channels.TryGetValue(targetName, out List<IChannel>? list);
            return list;
        }
    }
}