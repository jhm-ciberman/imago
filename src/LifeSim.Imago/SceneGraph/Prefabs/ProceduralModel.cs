using System;

namespace LifeSim.Imago.SceneGraph.Prefabs;

/// <summary>
/// Marks a class as a named procedural model, discoverable at startup via reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ProceduralModelAttribute : Attribute
{
    /// <summary>
    /// Gets the name used to reference this model in the <c>procedural:</c> URI scheme.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProceduralModelAttribute"/> class.
    /// </summary>
    /// <param name="name">The name used to reference this model (e.g. <c>"tavern-table"</c>).</param>
    public ProceduralModelAttribute(string name)
    {
        this.Name = name;
    }
}

/// <summary>
/// A stateless factory that creates <see cref="IInstantiable"/> prefabs for code-defined 3D models.
/// Implementations are discovered via the <see cref="ProceduralModelAttribute"/> and cached by the registry.
/// </summary>
public interface IProceduralModel
{
    /// <summary>
    /// Creates a prefab for this model using the given request parameters.
    /// </summary>
    /// <param name="request">The request containing the model name and query parameters.</param>
    /// <returns>An instantiable prefab.</returns>
    public IInstantiable CreatePrefab(ProceduralModelRequest request);
}
