using System;
using System.Collections.Generic;
using System.Linq;
using Imago.Templates;

namespace Imago;

/// <summary>
/// Represents a container for layers that are rendered and updated together.
/// </summary>
/// <remarks>
/// A screen is a collection of layers (both 3D and 2D). Show it with <see cref="Stage.Screen"/>.
/// While it is shown, its layers are on the stage.
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
    /// Occurs when this screen has been activated on a <see cref="Stage"/>.
    /// </summary>
    public event EventHandler? Mounted;

    /// <summary>
    /// Occurs when this screen is being deactivated from its <see cref="Stage"/>.
    /// </summary>
    public event EventHandler? Unmounting;

    private bool _disposedValue;

    private IDisposable? _bindings;

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

        this._stage?.RemoveLayer(layer);
    }

    /// <summary>
    /// Finds the first layer of the given type, in the order the layers were added.
    /// </summary>
    /// <typeparam name="T">The type of layer to find.</typeparam>
    /// <returns>The first layer of type <typeparamref name="T"/>, or null if the screen has none.</returns>
    public T? FindLayer<T>() where T : class, ILayer
    {
        return this._layers.FirstOrDefault(layer => layer is T) as T;
    }

    /// <summary>
    /// Finds every layer of the given type, in the order the layers were added.
    /// </summary>
    /// <typeparam name="T">The type of layer to find.</typeparam>
    /// <returns>The layers of type <typeparamref name="T"/>. Empty if the screen has none.</returns>
    public IEnumerable<T> FindLayers<T>() where T : class, ILayer
    {
        return this._layers.OfType<T>();
    }

    /// <summary>
    /// Finds every visible layer of the given type, in the order the layers were added.
    /// </summary>
    /// <typeparam name="T">The type of layer to find.</typeparam>
    /// <returns>The visible layers of type <typeparamref name="T"/>. Empty if the screen has none.</returns>
    public IEnumerable<T> FindVisibleLayers<T>() where T : class, ILayer
    {
        return this._layers.OfType<T>().Where(layer => layer.IsVisible);
    }

    /// <summary>
    /// Mounts this screen on the given stage, pushing its layers.
    /// </summary>
    /// <param name="stage">The stage to mount to.</param>
    internal void Mount(Stage stage)
    {
        this._stage = stage;

        foreach (var layer in this._layers)
        {
            stage.AddLayer(layer);
        }

        stage.NotifyItemMounted(this);
        this._bindings = this.CreateBindings();
        this.OnMounted();
        this.Mounted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the screen is mounted to a <see cref="Stage"/>. Override to set up
    /// reactive bindings whose lifetime should match the screen's mount/unmount cycle.
    /// The returned disposable is disposed automatically when the screen is unmounted.
    /// </summary>
    /// <returns>The bindings, or <see langword="null"/> if there are none.</returns>
    protected virtual IDisposable? CreateBindings()
    {
        return null;
    }

    /// <summary>
    /// Unmounts this screen from its stage, removing its layers.
    /// </summary>
    internal void Unmount()
    {
        this.Unmounting?.Invoke(this, EventArgs.Empty);
        this.OnUnmounted();
        this._bindings?.Dispose();
        this._bindings = null;

        var stage = this._stage!;
        foreach (var layer in this._layers)
        {
            stage.RemoveLayer(layer);
        }

        stage.NotifyItemUnmounted(this);
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
    /// Disposes the screen and all its layers.
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
