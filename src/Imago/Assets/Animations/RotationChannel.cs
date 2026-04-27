using System.Numerics;

namespace Imago.Assets.Animations;

/// <summary>
/// Animates the local rotation component of a bone.
/// </summary>
public class RotationChannel : ChannelBase<Quaternion>
{
    private readonly QuaternionSampler _sampler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RotationChannel"/> class.
    /// </summary>
    /// <param name="targetName">The name of the bone to animate.</param>
    /// <param name="times">An array of keyframe times.</param>
    /// <param name="values">An array of quaternion rotation values corresponding to the keyframe times.</param>
    /// <param name="interpolation">The interpolation mode to use between keyframes.</param>
    public RotationChannel(string targetName, float[] times, Quaternion[] values, InterpolationMode interpolation) : base(targetName)
    {
        this._sampler = new QuaternionSampler(times, values, interpolation);
    }

    /// <inheritdoc/>
    public override float Duration => this._sampler.Duration;

    /// <inheritdoc/>
    public override void Sample(Pose pose, float time)
    {
        BoneTransform transform = pose.GetOrAdd(this.TargetName);
        transform.Rotation = this._sampler.Sample(time);
        pose.Set(this.TargetName, transform);
    }
}
