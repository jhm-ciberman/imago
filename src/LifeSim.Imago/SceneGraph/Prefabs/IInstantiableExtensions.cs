using System.Numerics;
using LifeSim.Imago.Materials;
using LifeSim.Imago.SceneGraph.Nodes;

namespace LifeSim.Imago.SceneGraph.Prefabs;

/// <summary>
/// Provides extension methods for instantiable objects to simplify creation with materials and transformations.
/// </summary>
public static class IInstantiableExtensions
{
    /// <summary>
    /// Instantiates the instantiable and sets the material for all meshes in a recursive fashion.
    /// </summary>
    /// <param name="instantiable">The instantiable resource.</param>
    /// <param name="material">The material to set.</param>
    /// <param name="textureST">The texture ST vector.</param>
    public static Node3D Instantiate(this IInstantiable instantiable, Material material, Vector4? textureST = null)
    {
        var node = instantiable.Instantiate();
        node.SetMaterial(material, textureST ?? new Vector4(1.0f, 1.0f, 0.0f, 0.0f));
        return node;
    }

    /// <summary>
    /// Wraps the instantiable in a <see cref="WrapingPrefab"/> if it is not already a <see cref="WrapingPrefab"/>.
    /// If the instantiable is already a <see cref="WrapingPrefab"/> it is mutated to the new offset and scale.
    /// </summary>
    /// <param name="instantiable">The instantiable resource.</param>
    /// <param name="offset">The offset to use.</param>
    /// <param name="scale">The scale to use.</param>
    /// <returns>The wrapped instantiable.</returns>
    public static IInstantiable Wrap(this IInstantiable instantiable, Vector3 offset, Vector3? scale = null)
    {
        return WrapingPrefab.WrapIfNecessary(instantiable, offset, scale);
    }
}
