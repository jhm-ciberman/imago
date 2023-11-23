using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.Animations;

public interface IChannel
{
    string TargetName { get; }
    float Duration { get; }
    void Update(Node3D target, float time);
}
