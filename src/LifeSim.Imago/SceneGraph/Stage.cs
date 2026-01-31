using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Imago.Assets.Textures;
using LifeSim.Imago.Controls;
using LifeSim.Imago.Input;
using LifeSim.Imago.SceneGraph.Cameras;
using LifeSim.Support.Drawing;

namespace LifeSim.Imago.SceneGraph;

/// <summary>
/// Represents the root container that manages scenes and layers for rendering.
/// </summary>
/// <remarks>
/// Stage is the top-level container that:
/// - Manages the current scene and its layers
/// - Routes input events to layers
/// - Coordinates rendering through the Renderer
/// - Supports persistent layers that survive scene changes
/// </remarks>
public class Stage
{
    /// <summary>
    /// Occurs when the current scene changes.
    /// </summary>
    public event EventHandler<SceneChangedEventArgs>? SceneChanged;

    private class EmptyScene : Scene { }

    private static readonly Scene _emptyScene = new EmptyScene();

    private readonly List<ILayer> _persistentLayers = new();
    private readonly List<ILayer> _allLayers = new();
    private readonly List<IInputHandler> _inputHandlers = new();

    /// <summary>
    /// Gets the current scene being displayed.
    /// </summary>
    public Scene CurrentScene { get; private set; } = _emptyScene;

    /// <summary>
    /// Gets the primary 3D layer from the current scene, if any.
    /// </summary>
    public Layer3D? Layer3D => this.CurrentScene.Layer3D;

    /// <summary>
    /// Gets all layers sorted by ZOrder (including persistent and scene layers).
    /// </summary>
    public IReadOnlyList<ILayer> Layers => this._allLayers;

    /// <summary>
    /// Gets the default clear color when no layer provides one.
    /// </summary>
    public Color DefaultClearColor { get; set; } = Color.CoolGray;

    /// <summary>
    /// Gets or sets the scale factor for 2D/GUI layers, used to adjust for high-DPI displays.
    /// </summary>
    public Vector2 GuiScale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets the built-in tooltip layer for displaying tooltips above all other UI content.
    /// </summary>
    public TooltipLayer TooltipLayer { get; }

    /// <summary>
    /// Gets the built-in cursor layer for displaying the mouse cursor above all other content.
    /// </summary>
    public CursorLayer CursorLayer { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Stage"/> class.
    /// </summary>
    public Stage()
    {
        this.TooltipLayer = new TooltipLayer();
        this.CursorLayer = new CursorLayer();
        this.AddPersistentLayer(this.TooltipLayer);
        this.AddPersistentLayer(this.CursorLayer);
    }

    /// <summary>
    /// Subscribes to input events to enable input handling for the current scene.
    /// </summary>
    public void EnableInputHandling()
    {
        var input = InputManager.Instance;
        input.MouseButtonPressed += this.Input_MouseButtonPressed;
        input.MouseButtonReleased += this.Input_MouseButtonReleased;
        input.MouseWheelScrolled += this.Input_MouseWheelScrolled;
        input.KeyPressed += this.Input_KeyPressed;
        input.KeyReleased += this.Input_KeyReleased;
    }

    /// <summary>
    /// Unsubscribes from input events to disable input handling for the current scene.
    /// </summary>
    public void DisableInputHandling()
    {
        var input = InputManager.Instance;
        input.MouseButtonPressed -= this.Input_MouseButtonPressed;
        input.MouseButtonReleased -= this.Input_MouseButtonReleased;
        input.MouseWheelScrolled -= this.Input_MouseWheelScrolled;
        input.KeyPressed -= this.Input_KeyPressed;
        input.KeyReleased -= this.Input_KeyReleased;
    }

    private void Input_MouseButtonPressed(object? sender, MouseButtonEventArgs e)
    {
        this.HandleMousePressed(e);
    }

    private void Input_MouseButtonReleased(object? sender, MouseButtonEventArgs e)
    {
        this.HandleMouseReleased(e);
    }

    private void Input_MouseWheelScrolled(object? sender, MouseWheelEventArgs e)
    {
        this.HandleMouseWheelScrolled(e);
    }

    private void Input_KeyPressed(object? sender, KeyboardEventArgs e)
    {
        this.HandleKeyPressed(e);
    }

    private void Input_KeyReleased(object? sender, KeyboardEventArgs e)
    {
        this.HandleKeyReleased(e);
    }

    /// <summary>
    /// Adds a persistent layer that survives scene changes.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    public void AddPersistentLayer(ILayer layer)
    {
        layer.Stage = this;
        this._persistentLayers.Add(layer);
        this.RebuildLayerList();
    }

    /// <summary>
    /// Removes a persistent layer.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    public void RemovePersistentLayer(ILayer layer)
    {
        layer.Stage = null;
        this._persistentLayers.Remove(layer);
        this.RebuildLayerList();
    }

    /// <summary>
    /// Changes the current scene.
    /// </summary>
    /// <param name="scene">The new scene to display. If null, an empty scene is used.</param>
    public void ChangeScene(Scene? scene)
    {
        scene ??= _emptyScene;
        if (this.CurrentScene == scene) return;

        var oldScene = this.CurrentScene;

        oldScene.LayerAdded -= this.Scene_LayerAdded;
        oldScene.LayerRemoved -= this.Scene_LayerRemoved;

        foreach (var layer in oldScene.Layers)
        {
            layer.Stage = null;
        }

        oldScene.OnDeactivated();

        if (oldScene != _emptyScene)
        {
            oldScene.Dispose();
        }

        this.CurrentScene = scene;

        scene.LayerAdded += this.Scene_LayerAdded;
        scene.LayerRemoved += this.Scene_LayerRemoved;

        foreach (var layer in scene.Layers)
        {
            layer.Stage = this;
        }

        scene.OnActivated();

        this.RebuildLayerList();
        this.SceneChanged?.Invoke(this, new SceneChangedEventArgs(oldScene, scene));
    }

    private void Scene_LayerAdded(object? sender, LayerChangedEventArgs e)
    {
        e.Layer.Stage = this;
        this.RebuildLayerList();
    }

    private void Scene_LayerRemoved(object? sender, LayerChangedEventArgs e)
    {
        e.Layer.Stage = null;
        this.RebuildLayerList();
    }

    private void RebuildLayerList()
    {
        this._allLayers.Clear();
        this._allLayers.AddRange(this._persistentLayers);
        this._allLayers.AddRange(this.CurrentScene.Layers);
        this._allLayers.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));

        this._inputHandlers.Clear();
        foreach (var layer in this._allLayers)
        {
            if (layer is IInputHandler handler)
            {
                this._inputHandlers.Add(handler);
            }
        }
    }

    /// <summary>
    /// Handles mouse button press events by routing to input handlers in reverse Z-order.
    /// </summary>
    /// <param name="e">The mouse button event arguments.</param>
    public void HandleMousePressed(MouseButtonEventArgs e)
    {
        for (int i = this._inputHandlers.Count - 1; i >= 0; i--)
        {
            var handler = this._inputHandlers[i];
            if (handler.IsVisible)
            {
                handler.HandleMousePressed(e);
                if (e.Handled) return;
            }
        }
    }

    /// <summary>
    /// Handles mouse button release events by routing to input handlers in reverse Z-order.
    /// </summary>
    /// <param name="e">The mouse button event arguments.</param>
    public void HandleMouseReleased(MouseButtonEventArgs e)
    {
        for (int i = this._inputHandlers.Count - 1; i >= 0; i--)
        {
            var handler = this._inputHandlers[i];
            if (handler.IsVisible)
            {
                handler.HandleMouseReleased(e);
                if (e.Handled) return;
            }
        }
    }

    /// <summary>
    /// Handles mouse wheel scroll events by routing to input handlers in reverse Z-order.
    /// </summary>
    /// <param name="e">The mouse wheel event arguments.</param>
    public void HandleMouseWheelScrolled(MouseWheelEventArgs e)
    {
        for (int i = this._inputHandlers.Count - 1; i >= 0; i--)
        {
            var handler = this._inputHandlers[i];
            if (handler.IsVisible)
            {
                handler.HandleMouseWheel(e);
                if (e.Handled) return;
            }
        }
    }

    /// <summary>
    /// Handles key press events by routing to input handlers in reverse Z-order.
    /// </summary>
    /// <param name="e">The keyboard event arguments.</param>
    public void HandleKeyPressed(KeyboardEventArgs e)
    {
        for (int i = this._inputHandlers.Count - 1; i >= 0; i--)
        {
            var handler = this._inputHandlers[i];
            if (handler.IsVisible)
            {
                handler.HandleKeyPressed(e);
                if (e.Handled) return;
            }
        }
    }

    /// <summary>
    /// Handles key release events by routing to input handlers in reverse Z-order.
    /// </summary>
    /// <param name="e">The keyboard event arguments.</param>
    public void HandleKeyReleased(KeyboardEventArgs e)
    {
        for (int i = this._inputHandlers.Count - 1; i >= 0; i--)
        {
            var handler = this._inputHandlers[i];
            if (handler.IsVisible)
            {
                handler.HandleKeyReleased(e);
                if (e.Handled) return;
            }
        }
    }

    /// <summary>
    /// Updates all layers and the current scene.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        // Update 3D layer with picking state
        if (this.Layer3D != null)
        {
            var isCursorOverGui = this.IsCursorOverGui();
            this.Layer3D.Picking.Update(this.Layer3D.Camera, isCursorOverGui);
        }

        this.CurrentScene.Update(deltaTime);

        foreach (var layer in this._allLayers)
        {
            layer.Update(deltaTime);
        }
    }

    /// <summary>
    /// Gets the camera from the primary 3D layer.
    /// </summary>
    public Camera? Camera => this.Layer3D?.Camera;

    /// <summary>
    /// Determines if the cursor is over any GUI element in any visible 2D layer.
    /// </summary>
    public bool IsCursorOverGui()
    {
        foreach (var layer in this._allLayers)
        {
            if (layer is ILayer2D layer2D && layer2D.IsVisible && layer2D.IsCursorOverElement)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Prepares all layers for rendering.
    /// </summary>
    /// <param name="renderTexture">The render texture that will be used for rendering.</param>
    public void PrepareForRender(RenderTexture renderTexture)
    {
        this.CurrentScene.RenderImGui();
        this.Layer3D?.PrepareForRender(renderTexture);
    }

    /// <summary>
    /// Gets the clear color for the stage.
    /// </summary>
    public Color? GetClearColor()
    {
        return this.Layer3D?.ClearColor ?? this.DefaultClearColor;
    }
}
