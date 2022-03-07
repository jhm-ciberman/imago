using System.Numerics;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim;

public class ScaleChannel : ChannelBase<Vector3>
{
    private readonly Vector3Sampler _sampler;

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
