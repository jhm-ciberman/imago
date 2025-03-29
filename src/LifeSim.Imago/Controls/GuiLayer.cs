using System;
using System.Diagnostics;
using System.Numerics;
using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering;
using LifeSim.Imago.Rendering.Sprites;
using LifeSim.Imago.SceneGraph;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

public class GuiLayer : ILayer2D
{
    /// <summary>
    /// Gets the <see cref="Viewport"/> of the <see cref="GuiLayer"/>.
    /// </summary>
    public Viewport Viewport { get; }

    private float _zoom = 1f;

    /// <summary>
    /// Gets or sets the global zoom of the <see cref="GuiLayer"/>.
    /// This will scale all controls on the <see cref="GuiLayer"/> by the given factor.
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
    /// Gets or sets a value indicating the used render scale.
    /// </summary>
    public Vector2 RenderScale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets a value indicating whether each control should be snapped to pixels.
    /// </summary>
    public bool SnapToPixels { get; set; } = true;

    /// <summary>
    /// Gets the <see cref="InputManager"/> used to handle input events.
    /// </summary>
    public InputManager Input { get; }

    /// <summary>
    /// Gets the time it took to measure and arrange the content in the last frame.
    /// </summary>
    public TimeSpan MeasureArrangeTime { get; private set; }

    private readonly Stopwatch _measureArrangeStopwatch = new Stopwatch();

    /// <summary>
    /// Initializes a new instance of the <see cref="GuiLayer"/> class.
    /// </summary>
    /// <param name="viewport">The viewport to use.</param>
    public GuiLayer(Viewport? viewport = null)
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
    /// Gets or sets the content of the <see cref="GuiLayer"/>.
    /// </summary>
    public Control? Content
    {
        get => this._content;
        set
        {
            if (this._content != value)
            {
                this._content?.OnRemovedFromStage(this);

                this._content = value;

                this._content?.OnAddedToStage(this);
            }
        }
    }

    /// <summary>
    /// Draws the gui layer.
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
    }

    /// <summary>
    /// Updates the gui layer.
    /// </summary>
    /// <param name="deltaTime">The time since the last update.</param>
    public virtual void Update(float deltaTime)
    {
        if (this._content is null) return;

        this._content.Update(deltaTime);
    }

    /// <summary>
    /// Finds a child control of the specified type by its name recursively.
    /// </summary>
    /// <typeparam name="T">The type of the control to find.</typeparam>
    /// <param name="name">The name of the control to find.</param>
    /// <returns>The control if found, otherwise null.</returns>
    public T? Find<T>(string name) where T : Visual
    {
        if (this.Content == null) return null;
        return this.Content.Find<T>(name);
    }

    /// <summary>
    /// Finds a child control of the specified type by its name recursively. Throws an exception if the control could not be found.
    /// </summary>
    /// <typeparam name="T">The type of the control to find.</typeparam>
    /// <param name="name">The name of the control to find.</param>
    /// <returns>The found control.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the control could not be found.</exception>
    public T FindOrFail<T>(string name) where T : Visual
    {
        if (this.Content == null) throw new InvalidOperationException("Content is null");
        return this.Content.FindOrFail<T>(name);
    }

    /// <summary>
    /// Converts a point from window space to viewport space.
    /// </summary>
    /// <param name="mousePosition">The point in window space.</param>
    /// <returns>The point in viewport space.</returns>
    public Vector2 WindowToViewport(Vector2 mousePosition)
    {
        return (mousePosition - this.Viewport.Position) / this.RenderScale;
    }
}
