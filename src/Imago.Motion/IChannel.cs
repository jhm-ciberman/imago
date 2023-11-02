using Imago.SceneGraph;

namespace Imago.Motion;

public interface IChannel
{
    string TargetName { get; }
    float Duration { get; }
    void Update(Node3D target, float time);
}
