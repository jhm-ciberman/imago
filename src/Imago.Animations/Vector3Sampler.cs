using System.Numerics;

namespace Imago.Animations;

public class Vector3Sampler : SamplerBase<Vector3>
{
    public Vector3Sampler(float[] times, Vector3[] values, InterpolationMode interpolation) : base(times, values, interpolation)
    {
    }

    protected override Vector3 Linear(Vector3 prevValue, Vector3 nextValue, float t)
    {
        return Vector3.Lerp(prevValue, nextValue, t);
    }

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
