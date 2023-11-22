using System.Numerics;
using Imago.SceneGraph.Nodes;

namespace Imago.SceneGraph.Prefabs;

public class WrapingPrefab : IInstantiable
{
    /// <summary>
    /// Gets or sets the origin of the model.
    /// </summary>
    public Vector3 Offset { get; set; } = Vector3.Zero;

    /// <summary>
    /// Gets or sets the scale of the model.
    /// </summary>
    public Vector3 Scale { get; set; } = Vector3.One;

    private readonly IInstantiable _prefab;

    /// <summary>
    /// Initializes a new instance of the <see cref="WrapingPrefab"/> class.
    /// </summary>
    /// <param name="prefab">The prefab to wrap.</param>
    public WrapingPrefab(IInstantiable prefab)
    {
        this._prefab = prefab;
    }


    public Node3D Instantiate()
    {
        var node = new Node3D();
        var child = this._prefab.Instantiate();
        child.Scale = this.Scale;
        child.Position = -this.Offset;
        node.AddChild(child);
        return node;
    }

    /// <summary>
    /// Wraps the given prefab in an <see cref="WrapingPrefab"/> if necessary.
    /// </summary>
    /// <param name="prefab">The prefab to wrap.</param>
    /// <param name="offset">The offset to apply.</param>
    /// <param name="scale">The scale to apply.</param>
    /// <returns>The wrapped prefab.</returns>
    public static IInstantiable WrapIfNecessary(IInstantiable prefab, Vector3 offset, Vector3? scale = null)
    {
        scale ??= Vector3.One;
        if (offset == Vector3.Zero && scale == Vector3.One)
            return prefab;

        if (prefab is WrapingPrefab offsetPrefab) // Mutable, but whatever.
        {
            offsetPrefab.Offset += offset;
            offsetPrefab.Scale *= scale.Value;
            return offsetPrefab;
        }

        return new WrapingPrefab(prefab)
        {
            Offset = offset,
            Scale = scale.Value,
        };
    }
}
