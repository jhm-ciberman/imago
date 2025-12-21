using System;
using System.Diagnostics;
using System.Numerics;
using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Imago.SceneGraph;
using LifeSim.Support.Drawing;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents the root of a 2D user interface, managing the layout, rendering, and input for a tree of <see cref="Control"/> objects.
/// </summary>
public class LayerUI : ILayer2D
{
    /// <inheritdoc />
    public int ZOrder { get; init; } = 100;

    /// <inheritdoc />
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Gets the viewport that this GUI layer is rendered to.
    /// </summary>
    public Viewport Viewport { get; }

    private float _zoom = 1f;

    /// <summary>
    /// Gets or sets the global zoom factor for the GUI layer.
    /// </summary>
    public float Zoom
    {
        get => this._zoom;
        set
        {
            if (this._zoom == value) return;
            this._zoom = value;
            this.Content?.InvalidateMeasure();
        }
    }

    /// <summary>
    /// Gets or sets the render scale, used to adjust for high-DPI displays.
    /// </summary>
    public Vector2 RenderScale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets a value indicating whether control positions and sizes should be snapped to the nearest pixel.
    /// </summary>
    public bool SnapToPixels { get; set; } = true;

    /// <summary>
    /// Gets or sets the custom cursor to display when the mouse is over this layer.
    /// </summary>
    public Cursor? Cursor { get; set; } = null;

    /// <summary>
    /// Gets or sets the scale of the custom cursor.
    /// </summary>
    public float CursorScale { get; set; } = .5f;

    /// <summary>
    /// Gets or sets a value indicating whether the custom cursor is enabled.
    /// </summary>
    public bool IsCustomCursorEnabled { get; set; } = true;

    /// <summary>
    /// Gets the <see cref="InputManager"/> instance used by this layer.
    /// </summary>
    public InputManager Input { get; }

    /// <summary>
    /// Gets the time it took to measure and arrange the content in the last frame.
    /// </summary>
    public TimeSpan MeasureArrangeTime { get; private set; }

    private readonly Stopwatch _measureArrangeStopwatch = new Stopwatch();

    /// <summary>
    /// Initializes a new instance of the <see cref="LayerUI"/> class.
    /// </summary>
    /// <param name="viewport">The viewport to render the GUI to. If null, the default GUI viewport is used.</param>
    public LayerUI(Viewport? viewport = null)
    {
        this.Input = InputManager.Instance;
        this.Viewport = viewport ?? Renderer.Instance.GuiViewport;
        this.Viewport.SizeChanged += this.Viewport_SizeChanged;
    }

    private void Viewport_SizeChanged(object? sender, EventArgs e)
    {
        this.Content?.InvalidateMeasure();
    }

    private Control? _content;

    /// <summary>
    /// Gets or sets the root control of the GUI layer.
    /// </summary>
    public Control? Content
    {
        get => this._content;
        set
        {
            if (this._content != value)
            {
                this._content?.OnRemovedFromLayer(this);

                this._content = value;

                this._content?.OnAddedToLayer(this);
            }
        }
    }

    /// <summary>
    /// Draws the GUI layer and all its visible controls.
    /// </summary>
    /// <param name="ctx">The drawing context to use.</param>
    public void Draw(DrawingContext ctx)
    {
        if (this._content is null) return;

        this._measureArrangeStopwatch.Restart();
        Vector2 size = this.Viewport.Size / this.Zoom;

        this._content.Measure(size);
        this._content.Arrange(new Rect(Vector2.Zero, size));

        this.MeasureArrangeTime = this._measureArrangeStopwatch.Elapsed;

        var position = this.Viewport.Position;
        var viewProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(position.X, size.X, size.Y, position.Y, -10f, 100f);

        ctx.SetViewProjectionMatrix(viewProjectionMatrix);
        this._content.Draw(ctx);

        // Draw tooltips after content but before cursor
        TooltipService.Instance.Draw(ctx);

        // Draw custom cursor if set and cursor is visible
        if (this.IsCustomCursorEnabled)
        {
            this.DrawCursor(ctx);
        }
    }

    private void DrawCursor(DrawingContext ctx)
    {
        if (this.Cursor == null) return;

        var mousePosition = this.WindowToViewport(this.Input.CursorPosition);

        // Draw the cursor texture
        Vector2 cursorSize = this.Cursor.TextureSize * this.CursorScale;
        Vector2 hotspot = this.Cursor.HotspotPixels * this.CursorScale;
        var cursorPosition = mousePosition - hotspot;
        ctx.DrawTexture(this.Cursor.Texture, cursorPosition, cursorSize, Vector2.Zero, Vector2.One, Color.White);
    }

    /// <summary>
    /// Gets the control that is currently under the mouse cursor.
    /// </summary>
    public Control? ControlUnderCursor { get; private set; } = null;

    /// <summary>
    /// Updates the state of the GUI layer and its controls.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update, in seconds.</param>
    public virtual void Update(float deltaTime)
    {
        if (this._content is null) return;

        this._content.Update(deltaTime);

        // Update tooltip service after content update
        TooltipService.Instance.Update(deltaTime);

        var position = this.WindowToViewport(this.Input.CursorPosition);

        this.ControlUnderCursor = this._content.HitTest(position);
        this.IsCursorOverElement = this.ControlUnderCursor != null;
    }

    /// <summary>
    /// Gets a value indicating whether the mouse cursor is currently over any element in this layer.
    /// </summary>
    public bool IsCursorOverElement { get; private set; } = false;

    /// <summary>
    /// Handles mouse button press events and dispatches them to the appropriate control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    public void HandleMousePressed(MouseButtonEventArgs e)
    {
        this.ControlUnderCursor?.HandleMousePressed(e);
    }

    /// <summary>
    /// Handles mouse button release events and dispatches them to the appropriate control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    public void HandleMouseReleased(MouseButtonEventArgs e)
    {
        this.ControlUnderCursor?.HandleMouseReleased(e);
    }

    /// <summary>
    /// Handles mouse wheel scroll events and dispatches them to the appropriate control.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    public void HandleMouseWheel(MouseWheelEventArgs e)
    {
        this.ControlUnderCursor?.HandleMouseWheel(e);
    }

    /// <summary>
    /// Handles key press events.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    public void HandleKeyPressed(KeyboardEventArgs e)
    {
        //
    }

    /// <summary>
    /// Handles key release events.
    /// </summary>
    /// <param name="e">The event arguments.</param>
    public void HandleKeyReleased(KeyboardEventArgs e)
    {
        //
    }

    /// <summary>
    /// Finds a control of the specified type by its name within the GUI layer.
    /// </summary>
    /// <typeparam name="T">The type of the control to find.</typeparam>
    /// <param name="name">The name of the control to find.</param>
    /// <returns>The control if found; otherwise, null.</returns>
    public T? Find<T>(string name) where T : Visual
    {
        if (this.Content == null) return null;
        return this.Content.Find<T>(name);
    }

    /// <summary>
    /// Finds a control of the specified type by its name within the GUI layer, throwing an exception if not found.
    /// </summary>
    /// <typeparam name="T">The type of the control to find.</typeparam>
    /// <param name="name">The name of the control to find.</param>
    /// <returns>The found control.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the content is null or the control is not found.</exception>
    public T FindOrFail<T>(string name) where T : Visual
    {
        if (this.Content == null) throw new InvalidOperationException("Content is null");
        return this.Content.FindOrFail<T>(name);
    }

    /// <summary>
    /// Converts a point from window client space to this layer's viewport space.
    /// </summary>
    /// <param name="mousePosition">The point in window space.</param>
    /// <returns>The point in viewport space.</returns>
    public Vector2 WindowToViewport(Vector2 mousePosition)
    {
        return (mousePosition - this.Viewport.Position) / this.RenderScale;
    }
}
