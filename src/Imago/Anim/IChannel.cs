using Imago.SceneGraph;

namespace Imago.Anim;

public interface IChannel
{
    string TargetName { get; }
    float Duration { get; }
    void Update(Node3D target, float time);
}
