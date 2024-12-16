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
    public Viewport Viewport { get; }

    private Control? _content;

    /// <summary>
    /// Gets or sets the global zoom of the page. This will scale all controls on the page by the given factor.
    /// </summary>
    public float Zoom { get; set; } = 1f;

    /// <summary>
    /// Gets or sets a value indicating whether each control should be snapped to pixels.
    /// </summary>
    public bool SnapToPixels { get; set; } = true;

    /// <summary>
    /// Gets the <see cref="InputManager"/>.
    /// </summary>
    public InputManager Input { get; }

    public GuiLayer(Viewport? viewport = null)
    {
        this.Input = InputManager.Current;
        this.Viewport = viewport ?? Renderer.Instance.Viewport;
        this.Viewport.SizeChanged += this.Viewport_SizeChanged;
    }

    private void Viewport_SizeChanged(object? sender, EventArgs e)
    {
        this.Content?.InvalidateMeasure();
    }

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

    public TimeSpan MeasureArrangeTime { get; private set; }

    private readonly Stopwatch _measureArrangeStopwatch = new Stopwatch();

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

    public virtual void Update(float deltaTime)
    {
        if (this._content is null) return;

        this._content.Update(deltaTime);
    }

    public T? Find<T>(string name) where T : Visual
    {
        if (this.Content == null) return null;
        return this.Content.Find<T>(name);
    }

    public T FindOrFail<T>(string name) where T : Visual
    {
        if (this.Content == null) throw new InvalidOperationException("Content is null");
        return this.Content.FindOrFail<T>(name);
    }
}
