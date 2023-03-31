using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Resources;

public static class IInstantiableExtensions
{
    /// <summary>
    /// Instantiates the resource and sets the material for all meshes in a recursive fashion.
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
}
