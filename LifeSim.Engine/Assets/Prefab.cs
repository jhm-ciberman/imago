using System;
using System.Numerics;
using LifeSim.Engine.Resources;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Assets;

public abstract class Prefab : IInstantiable, IDisposable
{
    /// <summary>
    /// Gets or sets the name of the prefab.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the origin of the model.
    /// </summary>
    public Vector3 ModelOrigin { get; set; } = Vector3.Zero;

    /// <summary>
    /// Gets or sets the scale of the model.
    /// </summary>
    public Vector3 ModelScale { get; set; } = Vector3.One;


    public Node3D Instantiate()
    {
        // if origin is not zero or the scale is not one, we need to wrap the scene in a node
        if (this.ModelOrigin != Vector3.Zero || this.ModelScale != Vector3.One)
        {
            var node = new Node3D();
            var child = this.InstantiateCore();
            child.Scale = this.ModelScale;
            child.Position = -this.ModelOrigin;
            node.AddChild(child);
            return node;
        }
        else
        {
            return this.InstantiateCore();
        }
    }

    /// <summary>
    /// This method should be overridden by derived classes to instantiate the model.
    /// </summary>
    /// <returns>The root node of the scene.</returns>
    protected abstract Node3D InstantiateCore();

    /// <summary>
    /// This method should be overridden by derived classes to dispose of any resources.
    /// </summary>
    /// <param name="disposing">True if the method is called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        //
    }

    /// <summary>
    /// Disposes of any resources.
    /// </summary>
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="Prefab"/> class.
    /// </summary>
    ~Prefab()
    {
        this.Dispose(false);
    }
}
