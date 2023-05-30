using System.Numerics;

namespace Imago.Anim;

public class QuaternionSampler : SamplerBase<Quaternion>
{
    public QuaternionSampler(float[] times, Quaternion[] values, InterpolationMode interpolation) : base(times, values, interpolation)
    {
    }

    protected override Quaternion Linear(Quaternion prevValue, Quaternion nextValue, float t)
    {
        return Quaternion.Slerp(prevValue, nextValue, t);
    }

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
