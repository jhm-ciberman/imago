using System.Numerics;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Animations;

/// <summary>
/// Represents an animation channel that animates the scale of a <see cref="Node3D"/>.
/// </summary>
public class ScaleChannel : ChannelBase<Vector3>
{
    private readonly Vector3Sampler _sampler;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScaleChannel"/> class.
    /// </summary>
    /// <param name="targetName">The name of the target node to animate.</param>
    /// <param name="times">An array of keyframe times.</param>
    /// <param name="values">An array of scale values corresponding to the keyframe times.</param>
    /// <param name="interpolation">The interpolation mode to use between keyframes.</param>
    public ScaleChannel(string targetName, float[] times, Vector3[] values, InterpolationMode interpolation) : base(targetName)
    {
        this._sampler = new Vector3Sampler(times, values, interpolation);
    }

    public override float Duration => this._sampler.Duration;

    public override void Update(Node3D target, float time)
    {
        target.Scale = this._sampler.Sample(time);
    }
}
