using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Anim;

public interface IChannel
{
    string TargetName { get; }
    float Duration { get; }
    void Update(Node3D target, float time);
}
