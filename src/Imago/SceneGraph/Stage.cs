using System;
using System.Collections.Generic;
using System.Numerics;
using Imago.Assets.Textures;
using Imago.Controls;
using Imago.Input;
using Imago.SceneGraph.Cameras;
using Imago.Support.Drawing;

namespace Imago.SceneGraph;

/// <summary>
/// Represents the root container that manages screens and layers for rendering.
/// </summary>
/// <remarks>
/// Stage is the top-level container that:
/// - Manages the current screen and its layers
/// - Routes input events to layers
/// - Coordinates rendering through the Renderer
/// - Supports persistent layers that survive screen changes
/// </remarks>
public class Stage
{
    /// <summary>
    /// Occurs when the current screen changes.
    /// </summary>
    public event EventHandler<ScreenChangedEventArgs>? ScreenChanged;

    /// <summary>
    /// Occurs after the current screen's ImGui rendering, allowing global overlays
    /// to render regardless of the active screen.
    /// </summary>
    public event Action? ImGuiRendered;

    private class EmptyScreen : Screen { }

    private static readonly Screen _emptyScreen = new EmptyScreen();

    private readonly List<ILayer> _persistentLayers = new();
    private readonly List<ILayer> _allLayers = new();
    private readonly List<IInputHandler> _inputHandlers = new();

    /// <summary>
    /// Gets the current screen being displayed.
    /// </summary>
    public Screen CurrentScreen { get; private set; } = _emptyScreen;

    /// <summary>
    /// Gets the primary 3D scene from the current screen, if any.
    /// </summary>
    public Scene3D? Scene3D => this.CurrentScreen.Scene3D;

    /// <summary>
    /// Gets all layers sorted by ZOrder (including persistent and screen layers).
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
    /// Subscribes to input events to enable input handling for the current screen.
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
    /// Unsubscribes from input events to disable input handling for the current screen.
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
    /// Adds a persistent layer that survives screen changes.
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
    /// Changes the current screen.
    /// </summary>
    /// <param name="screen">The new screen to display. If null, an empty screen is used.</param>
    public void ChangeScreen(Screen? screen)
    {
        screen ??= _emptyScreen;
        if (this.CurrentScreen == screen) return;

        var oldScreen = this.CurrentScreen;

        oldScreen.LayerAdded -= this.Screen_LayerAdded;
        oldScreen.LayerRemoved -= this.Screen_LayerRemoved;

        foreach (var layer in oldScreen.Layers)
        {
            layer.Stage = null;
        }

        oldScreen.OnDeactivated();

        if (oldScreen != _emptyScreen)
        {
            oldScreen.Dispose();
        }

        this.CurrentScreen = screen;

        screen.LayerAdded += this.Screen_LayerAdded;
        screen.LayerRemoved += this.Screen_LayerRemoved;

        foreach (var layer in screen.Layers)
        {
            layer.Stage = this;
        }

        screen.OnActivated();

        this.RebuildLayerList();
        this.ScreenChanged?.Invoke(this, new ScreenChangedEventArgs(oldScreen, screen));
    }

    private void Screen_LayerAdded(object? sender, LayerChangedEventArgs e)
    {
        e.Layer.Stage = this;
        this.RebuildLayerList();
    }

    private void Screen_LayerRemoved(object? sender, LayerChangedEventArgs e)
    {
        e.Layer.Stage = null;
        this.RebuildLayerList();
    }

    private void RebuildLayerList()
    {
        this._allLayers.Clear();
        this._allLayers.AddRange(this._persistentLayers);
        this._allLayers.AddRange(this.CurrentScreen.Layers);
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
    /// Updates all layers and the current screen.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        this.UpdateInputBlocking();

        if (this.Scene3D != null)
        {
            var isCursorOverGui = this.IsCursorOverGui();
            this.Scene3D.Picking.Update(this.Scene3D.Camera, isCursorOverGui || this.Scene3D.IsInputBlocked);
        }

        this.CurrentScreen.Update(deltaTime);

        this.Scene3D?.Update(deltaTime);

        foreach (var layer in this._allLayers)
        {
            layer.Update(deltaTime);
        }
    }

    private void UpdateInputBlocking()
    {
        bool blocked = false;
        for (int i = this._allLayers.Count - 1; i >= 0; i--)
        {
            var layer = this._allLayers[i];
            layer.IsInputBlocked = blocked;

            if (layer is GuiLayer { IsVisible: true, BlocksInputBelow: true })
            {
                blocked = true;
            }
        }

        if (this.Scene3D != null)
        {
            this.Scene3D.IsInputBlocked = blocked;
        }
    }

    /// <summary>
    /// Gets the camera from the primary 3D scene.
    /// </summary>
    public Camera? Camera => this.Scene3D?.Camera;

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
        this.CurrentScreen.RenderImGui();
        this.ImGuiRendered?.Invoke();
        this.Scene3D?.PrepareForRender(renderTexture);
    }

    /// <summary>
    /// Gets the clear color for the stage.
    /// </summary>
    public Color? GetClearColor()
    {
        return this.Scene3D?.ClearColor ?? this.DefaultClearColor;
    }
}
