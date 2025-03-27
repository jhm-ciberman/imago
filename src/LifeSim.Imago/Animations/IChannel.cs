using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Animations;

public interface IChannel
{
    public string TargetName { get; }
    public float Duration { get; }
    public void Update(Node3D target, float time);
}
