using System;
using System.Diagnostics;
using System.Numerics;
using Imago.Graphics.Rendering;
using Imago.SceneGraph;
using Support;

namespace Imago.Controls;

public class GuiLayer : ILayer2D
{
    public Viewport Viewport { get; }

    private Control? _content;

    /// <summary>
    /// Gets or sets the global zoom of the page. This will scale all controls on the page by the given factor.
    /// </summary>
    public float Zoom { get; set; } = 1f;

    public Matrix4x4 ViewProjectionMatrix
    {
        get
        {
            Vector2 size = this.Viewport.Size / this.Zoom;
            return Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, -10f, 100f);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether each control should be snapped to pixels.
    /// </summary>
    public bool SnapToPixels { get; set; } = true;

    public GuiLayer(Viewport? viewport = null)
    {
        this.Viewport = viewport ?? Renderer.Instance.Viewport;
        this.Viewport.Resized += this.Viewport_Resized;
    }

    private void Viewport_Resized(object? sender, EventArgs e)
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

    public void Draw(SpriteBatcher spriteBatcher)
    {
        if (this._content is null) return;

        this._measureArrangeStopwatch.Restart();
        if (!this._content.IsArrangeValid || !this._content.IsMeasureValid)
        {

            Vector2 size = this.Viewport.Size / this.Zoom;
            this._content.Measure(size);
            this._content.Arrange(new Rect(0, 0, size.X, size.Y));
        }

        this.MeasureArrangeTime = this._measureArrangeStopwatch.Elapsed;

        this._content.Draw(spriteBatcher);
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
