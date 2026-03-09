using System;
using System.Collections.Generic;
using System.Linq;
using Imago.Controls;
using Imago.SceneGraph.Cameras;
using Imago.Support.Drawing;

namespace Imago.SceneGraph;

/// <summary>
/// Represents a container for layers that are rendered and updated together.
/// </summary>
/// <remarks>
/// A screen is a collection of layers (both 3D and 2D). When activated, its layers
/// and Scene3D are pushed onto the Stage. When deactivated, they are removed.
/// </remarks>
[ItemsMethod(nameof(AddLayer))]
public class Screen : IDisposable, IMountable
{
    private readonly List<ILayer> _layers = new();
    private Stage? _stage;

    /// <summary>
    /// Gets the list of layers in the screen.
    /// </summary>
    public IReadOnlyList<ILayer> Layers => this._layers;

    /// <summary>
    /// Gets or sets the 3D scene of the screen, if any.
    /// </summary>
    public Scene3D? Scene3D { get; set; }

    /// <summary>
    /// Gets the primary GUI layer of the screen (the first one added), if any.
    /// </summary>
    public GuiLayer? GuiLayer { get; private set; }

    /// <summary>
    /// Gets all GUI layers in the screen, ordered by ZOrder.
    /// </summary>
    public IEnumerable<GuiLayer> GuiLayers => this._layers.OfType<GuiLayer>().OrderBy(l => l.ZOrder);

    /// <summary>
    /// Gets or sets the clear color for the screen.
    /// This is a convenience property that sets ClearColor on the primary Scene3D.
    /// </summary>
    public Color? ClearColor
    {
        get => this.Scene3D?.ClearColor;
        set
        {
            if (this.Scene3D != null)
                this.Scene3D.ClearColor = value;
        }
    }

    /// <summary>
    /// Gets the environment from the primary Scene3D, if any.
    /// </summary>
    public SceneEnvironment? Environment => this.Scene3D?.Environment;

    /// <summary>
    /// Gets the camera from the primary Scene3D, if any.
    /// </summary>
    public Camera? Camera => this.Scene3D?.Camera;

    /// <summary>
    /// Occurs when this screen has been activated on a <see cref="Stage"/>.
    /// </summary>
    public event EventHandler? Mounted;

    /// <summary>
    /// Occurs when this screen is being deactivated from its <see cref="Stage"/>.
    /// </summary>
    public event EventHandler? Unmounting;

    private bool _disposedValue;

    /// <summary>
    /// Gets a value indicating whether this screen has been disposed.
    /// </summary>
    public bool IsDisposed => this._disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="Screen"/> class.
    /// </summary>
    public Screen()
    {
    }

    /// <summary>
    /// Adds a layer to the screen. If the screen is active, the layer is also added to the stage.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    public void AddLayer(ILayer layer)
    {
        if (layer is GuiLayer guiLayer)
        {
            this.GuiLayer ??= guiLayer;
        }

        this._layers.Add(layer);
        this._stage?.AddLayer(layer);
    }

    /// <summary>
    /// Removes a layer from the screen. If the screen is active, the layer is also removed from the stage.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    public void RemoveLayer(ILayer layer)
    {
        if (!this._layers.Remove(layer)) return;

        if (layer == this.GuiLayer)
        {
            this.GuiLayer = this._layers.OfType<GuiLayer>().FirstOrDefault();
        }

        this._stage?.RemoveLayer(layer);
    }

    /// <summary>
    /// Mounts this screen on the given stage, pushing its Scene3D and layers.
    /// </summary>
    /// <param name="stage">The stage to mount to.</param>
    public void Mount(Stage stage)
    {
        this._stage = stage;
        stage.Scene3D = this.Scene3D;

        foreach (var layer in this._layers)
        {
            stage.AddLayer(layer);
        }

        stage.ImGuiRendering += this.RenderImGui;
        this.OnMounted();
        this.Mounted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Unmounts this screen from its stage, removing its layers and Scene3D.
    /// </summary>
    public void Unmount()
    {
        this.Unmounting?.Invoke(this, EventArgs.Empty);
        this.OnUnmounted();
        this._stage!.ImGuiRendering -= this.RenderImGui;

        foreach (var layer in this._layers)
        {
            this._stage.RemoveLayer(layer);
        }

        this._stage.Scene3D = null;
        this._stage = null;
    }

    /// <summary>
    /// Called after the screen has been mounted to its stage.
    /// </summary>
    protected virtual void OnMounted()
    {
    }

    /// <summary>
    /// Called before the screen is unmounted from its stage.
    /// </summary>
    protected virtual void OnUnmounted()
    {
    }

    /// <summary>
    /// Updates the screen. Override this method to add custom screen logic.
    /// Layer updates are handled by the Stage.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public virtual void Update(float deltaTime)
    {
    }

    /// <summary>
    /// Renders the ImGui user interface for this screen.
    /// </summary>
    public virtual void RenderImGui()
    {
    }

    /// <summary>
    /// Disposes the screen and all its layers.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposedValue)
        {
            if (disposing)
            {
                this.Scene3D?.Dispose();

                foreach (var layer in this._layers)
                {
                    if (layer is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                this._layers.Clear();
            }

            this._disposedValue = true;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
