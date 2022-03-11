using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine;

public interface IScenePrefab
{
    string Name { get; }
    Node3D Instantiate();
}