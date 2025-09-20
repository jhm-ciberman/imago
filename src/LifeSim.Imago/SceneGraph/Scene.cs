using System.Collections.Generic;
using LifeSim.Imago.Input;
using LifeSim.Imago.SceneGraph.Cameras;
using LifeSim.Imago.SceneGraph.Nodes;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Represents a container for all 3D and 2D elements that are rendered and updated together.
/// </summary>
/// <remarks>
/// A scene is a <see cref="Node3D"/> that acts as the root of the scene graph. It holds the camera, environment settings,
/// particle systems, and a 2D GUI layer.
/// </remarks>
public class Scene : Node3D
{
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

    /// <summary>
    /// Gets or sets a value indicating whether the scene should be disposed when it is unloaded.
    /// </summary>
    public bool DisposeOnDetach { get; set; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scene"/> class.
    /// </summary>
    public Scene()
    {
        //
    }

    /// <summary>
    /// Called by the <see cref="Stage"/> before the scene is rendered to perform any necessary preparations.
    /// </summary>
    public virtual void PrepareForRender()
    {
        //
    }

    /// <summary>
    /// Renders the ImGui user interface for this scene.
    /// </summary>
    public virtual void RenderImGui()
    {
        // Virtual method
    }

    /// <summary>
    /// Updates the scene's state.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        this.GuiLayer?.Update(deltaTime);
    }

    private readonly List<IParticleSystem> _particleSystems = [];

    /// <summary>
    /// Gets the list of particle systems in the scene.
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
    /// Gets or sets the 2D GUI layer of the scene.
    /// </summary>
    public ILayer2D? GuiLayer { get; set; } = null;

    /// <inheritdoc/>
    public override void DetachFromStage()
    {
        base.DetachFromStage();

        if (this.DisposeOnDetach)
        {
            this.Dispose();
        }
    }

    /// <summary>
    /// Handles mouse button press events.
    /// </summary>
    /// <param name="e">The mouse button event arguments.</param>
    public void HandleMousePressed(MouseButtonEventArgs e)
    {
        this.GuiLayer?.HandleMousePressed(e);
    }

    /// <summary>
    /// Handles mouse button release events.
    /// </summary>
    /// <param name="e">The mouse button event arguments.</param>
    public void HandleMouseReleased(MouseButtonEventArgs e)
    {
        this.GuiLayer?.HandleMouseReleased(e);
    }

    /// <summary>
    /// Handles mouse wheel scroll events.
    /// </summary>
    /// <param name="e">The mouse wheel event arguments.</param>
    public void HandleMouseWheelScrolled(MouseWheelEventArgs e)
    {
        this.GuiLayer?.HandleMouseWheel(e);
    }

    /// <summary>
    /// Handles key press events.
    /// </summary>
    /// <param name="e">The keyboard event arguments.</param>
    public void HandleKeyPressed(KeyboardEventArgs e)
    {
        this.GuiLayer?.HandleKeyPressed(e);
    }

    /// <summary>
    /// Handles key release events.
    /// </summary>
    /// <param name="e">The keyboard event arguments.</param>
    public void HandleKeyReleased(KeyboardEventArgs e)
    {
        this.GuiLayer?.HandleKeyReleased(e);
    }
}
