using System;

namespace LifeSim.Imago.Animations;

/// <summary>
/// Provides a base class for interpolating animation values over time.
/// </summary>
/// <typeparam name="T">The type of value being sampled.</typeparam>
public abstract class SamplerBase<T> where T : struct
{
    /// <summary>
    /// Gets or sets the interpolation mode used for this sampler.
    /// </summary>
    public InterpolationMode Interpolation { get; set; } = InterpolationMode.Step;

    /// <summary>
    /// Gets the array of raw values for this sampler.
    /// For step and linear each time has only one value, (values.Length == times.Length)
    /// For cubic spline each time has tree values: in tangent, value, out tangent (values.Length == times.Length * 3)
    /// </summary>
    public T[] Values { get; }

    /// <summary>
    /// Gets the raw array of times for this sampler. The times are in seconds.
    /// </summary>
    public float[] Times { get; }

    /// <summary>
    /// Gets the duration of this sampler in seconds.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SamplerBase{T}"/> class.
    /// </summary>
    /// <param name="times">An array of keyframe times.</param>
    /// <param name="values">An array of values corresponding to the keyframe times.</param>
    /// <param name="interpolation">The interpolation mode to use between keyframes.</param>
    public SamplerBase(float[] times, T[] values, InterpolationMode interpolation)
    {
        this.Times = times;
        this.Values = values;
        this.Interpolation = interpolation;
        this.Duration = times[times.Length - 1];
    }

    /// <summary>
    /// Returns the index of the time that is equal or greater than the given time.
    /// </summary>
    /// <param name="time">The time to find the index for.</param>
    /// <returns>The index of the time that is equal or greater than the given time.</returns>
    public int FindNextIndex(float time)
    {
        var index = Array.BinarySearch(this.Times, time);
        if (index < 0)
        {
            index = ~index; // Get the index of the first element that is larger than the search value.
        }
        return Math.Clamp(index, 0, this.Times.Length);
    }

    /// <summary>
    /// Interpolates the value at the given time using linear interpolation.
    /// </summary>
    /// <param name="prevValue">The value at the previous time.</param>
    /// <param name="nextValue">The value at the next time.</param>
    /// <param name="t">The interpolation factor, between 0 and 1.</param>
    /// <returns>The interpolated value.</returns>
    protected abstract T Linear(T prevValue, T nextValue, float t);

    /// <summary>
    /// Interpolates the value at the given time using cubic spline interpolation.
    /// </summary>
    /// <param name="prevValue">The value at the previous time.</param>
    /// <param name="prevTangent">The tangent at the previous time.</param>
    /// <param name="nextValue">The value at the next time.</param>
    /// <param name="nextTangent">The tangent at the next time.</param>
    /// <param name="t">The interpolation factor, between 0 and 1.</param>
    /// <returns>The interpolated value.</returns>
    protected abstract T CubicSpline(T prevValue, T prevTangent, T nextValue, T nextTangent, float t);

    /// <summary>
    /// Samples the value at the given time.
    /// </summary>
    /// <param name="time">The time to sample the value at. Must be between 0 and Duration otherwise the result is undefined.</param>
    /// <returns>The sampled value.</returns>
    public T Sample(float time)
    {
        time = float.Clamp(time, 0, this.Duration);

        var index = this.FindNextIndex(time);
        var prevIndex = Math.Max(index - 1, 0);

        if (index == prevIndex)
        {
            return this.Values[prevIndex];
        }

        var prevTime = this.Times[prevIndex];
        var nextTime = this.Times[index];
        var prevValue = this.Values[prevIndex];
        var nextValue = this.Values[index];

        var t = (time - prevTime) / (nextTime - prevTime);

        switch (this.Interpolation)
        {
            case InterpolationMode.Step:
                return prevValue;
            case InterpolationMode.Linear:
                return this.Linear(prevValue, nextValue, t);
            case InterpolationMode.CubicSpline:
                var prevTangent = this.Values[prevIndex + 1];
                var nextTangent = this.Values[index + 1];
                return this.CubicSpline(prevValue, prevTangent, nextValue, nextTangent, t);
            default:
                throw new InvalidOperationException();
        }
    }
}
