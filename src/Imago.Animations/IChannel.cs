using Imago.SceneGraph.Nodes;

namespace Imago.Animations;

public interface IChannel
{
    string TargetName { get; }
    float Duration { get; }
    void Update(Node3D target, float time);
}
