using System;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Gltf;

public interface IScenePrefab : IDisposable
{
    string Name { get; }
    Node3D Instantiate();
}