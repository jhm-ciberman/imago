using System;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Gltf;

public interface IScenePrefab : IInstantiable, IDisposable
{
    /// <summary>
    /// Gets the name of the scene.
    /// </summary>
    string Name { get; }
}
