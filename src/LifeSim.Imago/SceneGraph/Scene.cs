using System;
using System.Collections.Generic;
using System.Linq;
using LifeSim.Imago.Controls;
using LifeSim.Imago.SceneGraph.Cameras;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Represents a container for layers that are rendered and updated together.
/// </summary>
/// <remarks>
/// A scene is a collection of layers (both 3D and 2D). When a scene is activated,
/// its layers are added to the Stage. When deactivated, they are removed.
/// </remarks>
public class Scene : IDisposable
{
    /// <summary>
    /// Occurs when a layer is added to the scene.
    /// </summary>
    public event EventHandler<LayerChangedEventArgs>? LayerAdded;

    /// <summary>
    /// Occurs when a layer is removed from the scene.
    /// </summary>
    public event EventHandler<LayerChangedEventArgs>? LayerRemoved;

    private readonly List<ILayer> _layers = new();

    /// <summary>
    /// Gets the list of layers in the scene.
    /// </summary>
    public IReadOnlyList<ILayer> Layers => this._layers;

    /// <summary>
    /// Gets the primary 3D layer of the scene, if any.
    /// </summary>
    public Layer3D? Layer3D { get; private set; }

    /// <summary>
    /// Gets the primary GUI layer of the scene (the first one added), if any.
    /// </summary>
    public GuiLayer? GuiLayer { get; private set; }

    /// <summary>
    /// Gets all GUI layers in the scene, ordered by ZOrder.
    /// </summary>
    public IEnumerable<GuiLayer> GuiLayers => this._layers.OfType<GuiLayer>().OrderBy(l => l.ZOrder);

    /// <summary>
    /// Gets or sets the clear color for the scene.
    /// This is a convenience property that sets ClearColor on the primary Layer3D.
    /// </summary>
    public Color? ClearColor
    {
        get => this.Layer3D?.ClearColor;
        set
        {
            if (this.Layer3D != null)
                this.Layer3D.ClearColor = value;
        }
    }

    /// <summary>
    /// Gets the environment from the primary Layer3D, if any.
    /// </summary>
    public SceneEnvironment? Environment => this.Layer3D?.Environment;

    /// <summary>
    /// Gets the camera from the primary Layer3D, if any.
    /// </summary>
    public Camera? Camera => this.Layer3D?.Camera;

    private bool _disposedValue;

    /// <summary>
    /// Gets a value indicating whether this scene has been disposed.
    /// </summary>
    public bool IsDisposed => this._disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="Scene"/> class.
    /// </summary>
    public Scene()
    {
    }

    /// <summary>
    /// Adds a layer to the scene.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to add a second Layer3D.
    /// </exception>
    public void AddLayer(ILayer layer)
    {
        if (layer is Layer3D layer3D)
        {
            if (this.Layer3D != null)
            {
                throw new InvalidOperationException("Scene already has a Layer3D. Only one Layer3D per scene is supported.");
            }

            this.Layer3D = layer3D;
        }
        else if (layer is GuiLayer guiLayer)
        {
            this.GuiLayer ??= guiLayer;
        }

        this._layers.Add(layer);
        this.LayerAdded?.Invoke(this, new LayerChangedEventArgs(layer));
    }

    /// <summary>
    /// Removes a layer from the scene.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    public void RemoveLayer(ILayer layer)
    {
        if (!this._layers.Remove(layer)) return;

        if (layer == this.Layer3D)
        {
            this.Layer3D = null;
        }
        else if (layer == this.GuiLayer)
        {
            this.GuiLayer = this._layers.OfType<GuiLayer>().FirstOrDefault();
        }

        this.LayerRemoved?.Invoke(this, new LayerChangedEventArgs(layer));
    }

    /// <summary>
    /// Called when the scene is activated (becomes the current scene).
    /// </summary>
    public virtual void OnActivated()
    {
    }

    /// <summary>
    /// Called when the scene is deactivated (no longer the current scene).
    /// </summary>
    public virtual void OnDeactivated()
    {
    }

    /// <summary>
    /// Updates the scene. Override this method to add custom scene logic.
    /// Layer updates are handled by the Stage.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public virtual void Update(float deltaTime)
    {
    }

    /// <summary>
    /// Renders the ImGui user interface for this scene.
    /// </summary>
    public virtual void RenderImGui()
    {
    }

    /// <summary>
    /// Disposes the scene and all its layers.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!this._disposedValue)
        {
            if (disposing)
            {
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
