using System.Numerics;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Assets.Animations;

/// <summary>
/// Represents an animation channel that animates the position of a <see cref="Node3D"/>.
/// </summary>
public class PositionChannel : ChannelBase<Vector3>, IChannel
{
    private readonly Vector3Sampler _sampler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PositionChannel"/> class.
    /// </summary>
    /// <param name="targetName">The name of the target node to animate.</param>
    /// <param name="times">An array of keyframe times.</param>
    /// <param name="values">An array of position values corresponding to the keyframe times.</param>
    /// <param name="interpolation">The interpolation mode to use between keyframes.</param>
    public PositionChannel(string targetName, float[] times, Vector3[] values, InterpolationMode interpolation) : base(targetName)
    {
        this._sampler = new Vector3Sampler(times, values, interpolation);
    }

    /// <inheritdoc/>
    public override float Duration => this._sampler.Duration;

    /// <inheritdoc/>
    public override void Update(Node3D target, float time)
    {
        target.Position = this._sampler.Sample(time);
    }
}
