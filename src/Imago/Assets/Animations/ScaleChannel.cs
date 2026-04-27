using System.Numerics;

namespace Imago.Assets.Animations;

/// <summary>
/// Animates the local scale component of a bone.
/// </summary>
public class ScaleChannel : ChannelBase<Vector3>
{
    private readonly Vector3Sampler _sampler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScaleChannel"/> class.
    /// </summary>
    /// <param name="targetName">The name of the bone to animate.</param>
    /// <param name="times">An array of keyframe times.</param>
    /// <param name="values">An array of scale values corresponding to the keyframe times.</param>
    /// <param name="interpolation">The interpolation mode to use between keyframes.</param>
    public ScaleChannel(string targetName, float[] times, Vector3[] values, InterpolationMode interpolation) : base(targetName)
    {
        this._sampler = new Vector3Sampler(times, values, interpolation);
    }

    /// <inheritdoc/>
    public override float Duration => this._sampler.Duration;

    /// <inheritdoc/>
    public override void Sample(Pose pose, float time)
    {
        BoneTransform transform = pose.GetOrAdd(this.TargetName);
        transform.Scale = this._sampler.Sample(time);
        pose.Set(this.TargetName, transform);
    }
}
