using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;
using LifeSim.Support;

namespace LifeSim.Engine.Controls;

public class StageUI
{
    public Viewport Viewport { get; }

    private Control? _content;

    /// <summary>
    /// Gets or sets the global zoom of the page. This will scale all controls on the page by the given factor.
    /// </summary>
    public float Zoom { get; set; } = 1f;

    private readonly HashSet<IAnimatedBrush> _animatedBrushes = new HashSet<IAnimatedBrush>();

    public Matrix4x4 ViewProjectionMatrix
    {
        get
        {
            Vector2 size = this.Viewport.Size / this.Zoom;
            return Matrix4x4.CreateOrthographicOffCenter(0, size.X, size.Y, 0, -10f, 100f);
        }
    }

    public StageUI(Viewport viewport)
    {
        this.Viewport = viewport;
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

    internal void Draw(SpriteBatcher spriteBatcher)
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

    internal void Update(float deltaTime)
    {
        if (this._content is null) return;

        foreach (var brush in this._animatedBrushes)
        {
            brush.Update(deltaTime);
        }

        this._content.Update(deltaTime);
    }

    internal void AddAnimatedBrush(IAnimatedBrush animatedBrush)
    {
        this._animatedBrushes.Add(animatedBrush);
    }

    internal void RemoveAnimatedBrush(IAnimatedBrush animatedBrush)
    {
        this._animatedBrushes.Remove(animatedBrush);
    }

    public T? GetElementByName<T>(string name) where T : Visual
    {
        if (this.Content == null) return null;
        return this.Content.GetElementByName<T>(name);
    }
}
