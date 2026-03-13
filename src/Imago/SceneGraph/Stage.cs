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
/// Represents the root container that manages layers and the 3D scene for rendering.
/// </summary>
/// <remarks>
/// Stage is the top-level container that:
/// <list type="bullet">
/// <item>Owns the 3D scene and all layers directly</item>
/// <item>Routes input events to layers</item>
/// <item>Coordinates rendering through the Renderer</item>
/// </list>
/// </remarks>
public class Stage
{
    /// <summary>
    /// Occurs during the render preparation phase, allowing the active screen to submit ImGui draw calls.
    /// </summary>
    public event Action? ImGuiRendering;

    /// <summary>
    /// Occurs after ImGui rendering, allowing global overlays
    /// to render regardless of the active screen.
    /// </summary>
    public event Action? ImGuiRendered;

    private readonly List<ILayer> _allLayers = new();
    private readonly List<ILayer2D> _guiLayers = new();
    private readonly List<ILayer2D> _overlayLayers = new();
    private readonly List<IInputHandler> _inputHandlers = new();
    private Scene3D? _scene3D;

    /// <summary>
    /// Gets or sets the primary 3D scene.
    /// </summary>
    /// <remarks>
    /// Setting this property automatically mounts/unmounts the scene graph.
    /// </remarks>
    public Scene3D? Scene3D
    {
        get => this._scene3D;
        set
        {
            if (this._scene3D == value) return;
            this._scene3D?.Unmount();
            this._scene3D = value;
            this._scene3D?.Mount(this);
        }
    }

    /// <summary>
    /// Gets all layers sorted by ZOrder.
    /// </summary>
    public IReadOnlyList<ILayer> Layers => this._allLayers;

    /// <summary>
    /// Gets 2D layers that render to the GUI render texture, sorted by ZOrder.
    /// </summary>
    public IReadOnlyList<ILayer2D> GuiLayers => this._guiLayers;

    /// <summary>
    /// Gets 2D layers that render as overlays on top of ImGui, sorted by ZOrder.
    /// </summary>
    public IReadOnlyList<ILayer2D> OverlayLayers => this._overlayLayers;

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
        this.AddLayer(this.TooltipLayer);
        this.AddLayer(this.CursorLayer);
    }

    /// <summary>
    /// Subscribes to input events to enable input handling.
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
    /// Unsubscribes from input events to disable input handling.
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
    /// Adds a layer to the stage.
    /// </summary>
    /// <param name="layer">The layer to add.</param>
    public void AddLayer(ILayer layer)
    {
        this._allLayers.Add(layer);
        layer.Mount(this);
        this.RebuildLayerList();
    }

    /// <summary>
    /// Removes a layer from the stage.
    /// </summary>
    /// <param name="layer">The layer to remove.</param>
    public void RemoveLayer(ILayer layer)
    {
        if (!this._allLayers.Remove(layer)) return;
        layer.Unmount();
        this.RebuildLayerList();
    }

    private void RebuildLayerList()
    {
        this._allLayers.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));

        this._inputHandlers.Clear();
        this._guiLayers.Clear();
        this._overlayLayers.Clear();

        foreach (var layer in this._allLayers)
        {
            if (layer is IInputHandler handler)
            {
                this._inputHandlers.Add(handler);
            }

            if (layer is ILayer2D layer2D)
            {
                if (layer2D.RenderTarget == LayerRenderTarget.Overlay)
                {
                    this._overlayLayers.Add(layer2D);
                }
                else
                {
                    this._guiLayers.Add(layer2D);
                }
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
    /// Updates all layers and the 3D scene.
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
        this.ImGuiRendering?.Invoke();
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
