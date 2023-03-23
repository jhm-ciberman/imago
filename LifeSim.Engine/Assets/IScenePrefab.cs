using System;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Assets;

public interface IScenePrefab : IInstantiable, IDisposable
{
    /// <summary>
    /// Gets the name of the scene.
    /// </summary>
    string Name { get; }
}
