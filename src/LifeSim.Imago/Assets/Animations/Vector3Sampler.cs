using System.Numerics;

namespace LifeSim.Imago.Assets.Animations;

/// <summary>
/// A sampler that interpolates <see cref="Vector3"/> values over time.
/// </summary>
public class Vector3Sampler : SamplerBase<Vector3>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Vector3Sampler"/> class.
    /// </summary>
    /// <param name="times">An array of keyframe times.</param>
    /// <param name="values">An array of Vector3 values corresponding to the keyframe times.</param>
    /// <param name="interpolation">The interpolation mode to use between keyframes.</param>
    public Vector3Sampler(float[] times, Vector3[] values, InterpolationMode interpolation) : base(times, values, interpolation)
    {
    }

    /// <summary>
    /// Performs linear interpolation between two Vector3 values.
    /// </summary>
    /// <param name="prevValue">The previous Vector3 value.</param>
    /// <param name="nextValue">The next Vector3 value.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated Vector3.</returns>
    protected override Vector3 Linear(Vector3 prevValue, Vector3 nextValue, float t)
    {
        return Vector3.Lerp(prevValue, nextValue, t);
    }

    /// <summary>
    /// Performs cubic spline interpolation between two Vector3 values.
    /// </summary>
    /// <param name="prevValue">The previous Vector3 value.</param>
    /// <param name="prevTangent">The tangent at the previous keyframe.</param>
    /// <param name="nextValue">The next Vector3 value.</param>
    /// <param name="nextTangent">The tangent at the next keyframe.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated Vector3.</returns>
    protected override Vector3 CubicSpline(Vector3 prevValue, Vector3 prevTangent, Vector3 nextValue, Vector3 nextTangent, float t)
    {
        var t2 = t * t;
        var t3 = t2 * t;
        return (2f * t3 - 3f * t2 + 1f) * prevValue
                + (t3 - 2f * t2 + t) * prevTangent
                + (-2f * t3 + 3f * t2) * nextValue
                + (t3 - t2) * nextTangent;
    }
}
