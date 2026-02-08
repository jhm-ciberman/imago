using System;
using LifeSim.Imago.Assets.Meshes;
using LifeSim.Imago.SceneGraph.Nodes;

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
/// Base class for code-defined 3D models that are discoverable via the <see cref="ProceduralModelAttribute"/>.
/// </summary>
/// <remarks>
/// <para>For simple single-mesh models, override <see cref="BuildMesh"/>. The mesh is built once and cached
/// automatically; each call to <see cref="Instantiate"/> returns a new <see cref="RenderNode3D"/> sharing the
/// same GPU mesh.</para>
/// <para>For complex scene graph hierarchies, override <see cref="Instantiate"/> directly and cache meshes
/// as fields using the <c>??=</c> pattern.</para>
/// </remarks>
public abstract class ProceduralModel : IInstantiable
{
    private Mesh? _cachedMesh;

    /// <summary>
    /// Builds the mesh for this model. Called once; the result is cached by the default <see cref="Instantiate"/>
    /// implementation.
    /// </summary>
    /// <returns>The mesh for this model.</returns>
    protected virtual Mesh BuildMesh()
    {
        throw new NotImplementedException(
            $"{this.GetType().Name} must override BuildMesh() or Instantiate()."
        );
    }

    /// <inheritdoc />
    public virtual Node3D Instantiate()
    {
        this._cachedMesh ??= this.BuildMesh();
        return new RenderNode3D(this._cachedMesh);
    }
}
