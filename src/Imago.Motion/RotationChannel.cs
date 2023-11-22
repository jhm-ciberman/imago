using System.Numerics;
using Imago.SceneGraph.Nodes;

namespace Imago.Motion;

public class RotationChannel : ChannelBase<Quaternion>
{
    private readonly QuaternionSampler _sampler;

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
