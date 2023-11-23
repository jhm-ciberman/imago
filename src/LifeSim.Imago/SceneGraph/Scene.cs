using System;
using System.Collections.Generic;
using System.Linq;
using LifeSim.Imago.SceneGraph.Cameras;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph;

public class Scene : Node3D
{
    private readonly List<ILayer2D> _layers2D = new();

    /// <summary>
    /// Gets or sets the clear color of the stage. If null, the stage will not be cleared
    /// and the previous frame will be visible.
    /// </summary>
    public Color? ClearColor { get; set; } = Color.Black;

    /// <summary>
    /// Gets or sets the camera used to render the scene.
    /// </summary>
    public Camera? Camera { get; set; } = null;

    /// <summary>
    /// Gets or sets the environment of the scene.
    /// </summary>
    public SceneEnvironment Environment { get; set; } = new SceneEnvironment();

    public Scene()
    {
        //
    }

    /// <summary>
    /// Called before the scene is rendered.
    /// </summary>
    public virtual void PrepareForRender()
    {
        //
    }

    public virtual void RenderImGui()
    {
        // Virtual method
    }

    public virtual void Update(float deltaTime)
    {
        foreach (var layer in this._layers2D)
        {
            layer.Update(deltaTime);
        }
    }

    private readonly List<IParticleSystem> _particleSystems = new List<IParticleSystem>();

    /// <summary>
    /// Gets the particle systems used to render particles.
    /// </summary>
    public IReadOnlyList<IParticleSystem> ParticleSystems => this._particleSystems;

    /// <summary>
    /// Adds a particle system to the scene.
    /// </summary>
    /// <param name="particleSystem">The particle system to add.</param>
    public void AddParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Add(particleSystem);
    }

    /// <summary>
    /// Removes a particle system from the scene.
    /// </summary>
    /// <param name="particleSystem">The particle system to remove.</param>
    public void RemoveParticleSystem(IParticleSystem particleSystem)
    {
        this._particleSystems.Remove(particleSystem);
    }

    /// <summary>
    /// Adds a 2D layer to the scene.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    public void AddLayer(ILayer2D layer)
    {
        this._layers2D.Add(layer);
    }

    /// <summary>
    /// Removes a 2D layer from the scene.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    public void RemoveLayer(ILayer2D layer)
    {
        this._layers2D.Remove(layer);
    }

    /// <summary>
    /// Gets the first layer of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the layer.</typeparam>
    /// <returns>The layer or null if no layer of the specified type exists.</returns>
    public T? GetLayer<T>()
    {
        return this._layers2D.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Gets the 2D layers of the scene.
    /// </summary>
    public IReadOnlyList<ILayer2D> Layers2D => this._layers2D;
}
