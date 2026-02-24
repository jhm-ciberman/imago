using System.Numerics;

namespace Imago.Assets.Animations;

/// <summary>
/// A sampler that interpolates <see cref="Quaternion"/> values over time.
/// </summary>
public class QuaternionSampler : SamplerBase<Quaternion>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QuaternionSampler"/> class.
    /// </summary>
    /// <param name="times">An array of keyframe times.</param>
    /// <param name="values">An array of quaternion values corresponding to the keyframe times.</param>
    /// <param name="interpolation">The interpolation mode to use between keyframes.</param>
    public QuaternionSampler(float[] times, Quaternion[] values, InterpolationMode interpolation) : base(times, values, interpolation)
    {
    }

    /// <summary>
    /// Performs spherical linear interpolation between two quaternion values.
    /// </summary>
    /// <param name="prevValue">The previous quaternion value.</param>
    /// <param name="nextValue">The next quaternion value.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated quaternion.</returns>
    protected override Quaternion Linear(Quaternion prevValue, Quaternion nextValue, float t)
    {
        return Quaternion.Slerp(prevValue, nextValue, t);
    }

    /// <summary>
    /// Performs cubic spline interpolation between two quaternion values.
    /// </summary>
    /// <param name="prevValue">The previous quaternion value.</param>
    /// <param name="prevTangent">The tangent at the previous keyframe.</param>
    /// <param name="nextValue">The next quaternion value.</param>
    /// <param name="nextTangent">The tangent at the next keyframe.</param>
    /// <param name="t">The interpolation factor.</param>
    /// <returns>The interpolated quaternion.</returns>
    protected override Quaternion CubicSpline(Quaternion prevValue, Quaternion prevTangent, Quaternion nextValue, Quaternion nextTangent, float t)
    {
        // first calculate the new "t" value using spline interpolation
        // (probably there is a more efficient way to do this, but I don't care as long as it works)
        var t2 = t * t;
        var t3 = t2 * t;
        var newT = (2f * t3 - 3f * t2 + 1f) * t
                + (t3 - 2f * t2 + t) * (1f / 6f)
                + (-2f * t3 + 3f * t2) * (1f / 2f)
                + t3 * (1f / 6f);

        // then calculate the new quaternion using spherical linear interpolation
        return Quaternion.Slerp(prevValue, nextValue, newT);
    }
}
