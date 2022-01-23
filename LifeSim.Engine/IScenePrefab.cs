using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine;

public interface IScenePrefab
{
    Node3D Instantiate(SceneStorage storage);
}