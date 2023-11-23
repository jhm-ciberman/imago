using System.Numerics;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Animations;

public class PositionChannel : ChannelBase<Vector3>, IChannel
{
    private readonly Vector3Sampler _sampler;
    public PositionChannel(string targetName, float[] times, Vector3[] values, InterpolationMode interpolation) : base(targetName)
    {
        this._sampler = new Vector3Sampler(times, values, interpolation);
    }

    public override float Duration => this._sampler.Duration;

    public override void Update(Node3D target, float time)
    {
        target.Position = this._sampler.Sample(time);
    }
}
