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
/// A screen is a collection of layers (both 3D and 2D). When a screen is activated,
/// its layers are added to the Stage. When deactivated, they are removed.
/// </remarks>
public class Screen : IDisposable
{
    /// <summary>
    /// Occurs when a layer is added to the screen.
    /// </summary>
    public event EventHandler<LayerChangedEventArgs>? LayerAdded;

    /// <summary>
    /// Occurs when a layer is removed from the screen.
    /// </summary>
    public event EventHandler<LayerChangedEventArgs>? LayerRemoved;

    private readonly List<ILayer> _layers = new();

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
    /// Adds a layer to the screen.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    public void AddLayer(ILayer layer)
    {
        if (layer is GuiLayer guiLayer)
        {
            this.GuiLayer ??= guiLayer;
        }

        this._layers.Add(layer);
        this.LayerAdded?.Invoke(this, new LayerChangedEventArgs(layer));
    }

    /// <summary>
    /// Removes a layer from the screen.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    public void RemoveLayer(ILayer layer)
    {
        if (!this._layers.Remove(layer)) return;

        if (layer == this.GuiLayer)
        {
            this.GuiLayer = this._layers.OfType<GuiLayer>().FirstOrDefault();
        }

        this.LayerRemoved?.Invoke(this, new LayerChangedEventArgs(layer));
    }

    /// <summary>
    /// Called when the screen is activated (becomes the current screen).
    /// </summary>
    /// <remarks>
    /// Subclasses that override this method must call <c>base.OnActivated()</c> to ensure
    /// the 3D scene graph is properly mounted.
    /// </remarks>
    public virtual void OnActivated()
    {
        this.Scene3D?.Mount();
    }

    /// <summary>
    /// Called when the screen is deactivated (no longer the current screen).
    /// </summary>
    /// <remarks>
    /// Subclasses that override this method must call <c>base.OnDeactivated()</c> to ensure
    /// the 3D scene graph is properly unmounted.
    /// </remarks>
    public virtual void OnDeactivated()
    {
        this.Scene3D?.Unmount();
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
