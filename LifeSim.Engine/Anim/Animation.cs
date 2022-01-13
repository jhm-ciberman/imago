using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim;

public class Animation
{
    public interface IChannel
    {
        string TargetName { get; }
        float Duration { get; }
        void UpdateTarget(Node3D target, float time, bool loop, ref int lastTimeIndex);
    }

    public abstract class BaseChannel<T> : IChannel where T : struct
    {
        private readonly ISampler<T> _sampler;
        public string TargetName { get; }

        public float Duration => this._sampler.Duration;

        public BaseChannel(string targetName, ISampler<T> sampler)
        {
            this._sampler = sampler;
            this.TargetName = targetName;
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
        protected override void SetTargetValue(Node3D node, ref Vector3 value)
        {
            node.Position = value;
        }
    }

    public class ScaleChannel : BaseChannel<Vector3>
    {
        public ScaleChannel(string targetName, ISampler<Vector3> sampler) : base(targetName, sampler) { }
        protected override void SetTargetValue(Node3D node, ref Vector3 value)
        {
            node.Scale = value;
        }
    }

    public class RotationChannel : BaseChannel<Quaternion>
    {
        public RotationChannel(string targetName, ISampler<Quaternion> sampler) : base(targetName, sampler) { }
        protected override void SetTargetValue(Node3D node, ref Quaternion value)
        {
            node.Rotation = value;
        }
    }

    public interface ISampler<T> where T : struct
    {
        float Duration { get; }
        T Sample(float time, bool loop, ref int lastTimeIndex);
    }

    public abstract class BaseSampler<T> : ISampler<T> where T : struct
    {
        protected float[] _times;
        protected T[] _values;

        public float Duration => this._times[^1];

        public BaseSampler(float[] times, T[] values)
        {
            this._times = times;
            this._values = values;
        }

        protected abstract T Interpolate(int indexPrev, int indexNext, float time);

#pragma warning disable IDE0060 //TODO: optimize with lastTimeIndex

        private int FindNextIndex(float time, int lastTimeIndex)
        {
            for (int i = 0; i < this._times.Length; i++)
            {
                if (this._times[i] >= time)
                {
                    return i;
                }
            }
            return this._times.Length - 1;
        }

#pragma warning restore

        public T Sample(float time, bool loop, ref int lastTimeIndex)
        {
            var nextIndex = this.FindNextIndex(time, lastTimeIndex);
            lastTimeIndex = nextIndex;

            //var prevIndex = loop 
            //    ? (nextIndex - 1) % this._times.Length 
            //    : nextIndex - 1;

            var prevIndex = System.Math.Clamp(nextIndex - 1, 0, this._times.Length);
            return this.Interpolate(prevIndex, nextIndex, time);
        }
    }

    public class SamplerLinear<T> : BaseSampler<T> where T : struct
    {
        protected IInterpolator<T> _interpolator;

        public SamplerLinear(float[] times, T[] values, IInterpolator<T> interpolator) : base(times, values)
        {
            this._interpolator = interpolator;
        }

        protected override T Interpolate(int indexPrev, int indexNext, float time)
        {
            var prev = this._values[indexPrev];
            var next = this._values[indexNext];
            var tPrev = this._times[indexPrev];
            var tNext = this._times[indexNext];

            if (indexPrev == indexNext)
            {
                return prev;
            }
            var t = (time - tPrev) / (tNext - tPrev);
            return this._interpolator.Linear(prev, next, t);
        }
    }

    public class SamplerStep<T> : BaseSampler<T> where T : struct
    {
        public SamplerStep(float[] times, T[] values) : base(times, values)
        {
        }

        protected override T Interpolate(int indexPrev, int indexNext, float time)
        {
            return this._values[indexPrev];
        }
    }

    public class SamplerCubicSpline<T> : BaseSampler<T> where T : struct
    {
        protected IInterpolator<T> _interpolator;

        public SamplerCubicSpline(float[] times, T[] values, IInterpolator<T> interpolator) : base(times, values)
        {
            this._interpolator = interpolator;
        }

        protected override T Interpolate(int indexPrev, int indexNext, float time)
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

    private readonly Dictionary<string, List<IChannel>> _channels = new Dictionary<string, List<IChannel>>();

    public float Duration { get; }

    public string Name { get; }

    public Animation(string name, IReadOnlyList<IChannel> channels)
    {
        this.Name = name;

        float duration = 0;
        foreach (var channel in channels)
        {
            duration = System.Math.Max(channel.Duration, duration);

            string key = channel.TargetName;
            key = key.Replace("mixamorig:", "");
            if (this._channels.TryGetValue(key, out List<IChannel>? list))
            {
                list.Add(channel);
            }
            else
            {
                this._channels.Add(key, new List<IChannel> { channel });
            }
        }
        this.Duration = duration;
    }

    public IEnumerable<string> ChannelNames => this._channels.Keys;

    public IReadOnlyList<IChannel>? FindChannels(string targetName)
    {
        this._channels.TryGetValue(targetName, out List<IChannel>? list);
        return list;
    }
}