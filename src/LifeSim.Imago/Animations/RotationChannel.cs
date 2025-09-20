using System.Numerics;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Animations;

/// <summary>
/// Represents an animation channel that animates the rotation of a <see cref="Node3D"/>.
/// </summary>
public class RotationChannel : ChannelBase<Quaternion>
{
    private readonly QuaternionSampler _sampler;

    /// <summary>
    /// Initializes a new instance of the <see cref="RotationChannel"/> class.
    /// </summary>
    /// <param name="targetName">The name of the target node to animate.</param>
    /// <param name="times">An array of keyframe times.</param>
    /// <param name="values">An array of quaternion rotation values corresponding to the keyframe times.</param>
    /// <param name="interpolation">The interpolation mode to use between keyframes.</param>
    public RotationChannel(string targetName, float[] times, Quaternion[] values, InterpolationMode interpolation) : base(targetName)
    {
        this._sampler = new QuaternionSampler(times, values, interpolation);
    }

    public override float Duration => this._sampler.Duration;

    public override void Update(Node3D target, float time)
    {
        target.Rotation = this._sampler.Sample(time);
    }
}
