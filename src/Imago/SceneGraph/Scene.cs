using System.Collections.Generic;
using Imago.Controls;
using Support;

namespace Imago.SceneGraph;

public class Scene : Node3D
{
    public GuiLayer? Gui { get; set; } = null;

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
        this.Gui?.Update(deltaTime);
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
}
