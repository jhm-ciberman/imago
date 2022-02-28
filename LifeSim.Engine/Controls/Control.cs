using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Controls;

public abstract class Control
{
    public string Name { get; set; } = string.Empty;

    public Action<object, float>? Updated { get; set; }

    public Thickness Margin { get; set; } = new Thickness(0);

    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;

    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

    public virtual IEnumerable<Control> VisualChildren { get; } = Enumerable.Empty<Control>();

    public Dock Dock { get; set; } = Dock.None;

    public Color BackgroundColor { get; set; } = Color.Transparent;

    public Control? Parent { get; internal set; }

    public Vector2 Position { get; protected set; } = Vector2.Zero;

    public Vector2 ActualSize { get; protected set; } = Vector2.Zero;

    public Visibility Visibility { get; protected set; } = Visibility.Visible;

    public Vector2 DesiredSize { get; protected set; } = Vector2.Zero;

    public float Width { get; set; } = float.NaN;

    public float Height { get; set; } = float.NaN;

    private Style? _style;

    public Style? Style
    {
        get => this._style;
        set
        {
            if (this._style != value)
            {
                this._style = value;
                value?.Apply(this);
            }
        }
    }

    public Control()
    {
        this.Style = Style.GetDefaultStyle(this.GetType());
    }

    public Control(string name) : this()
    {
        this.Name = name;
    }

    public void Measure(Vector2 availableSize)
    {
        if (this.Visibility == Visibility.Collapsed)
        {
            this.DesiredSize = Vector2.Zero;
            return;
        }

        var margin = new Vector2(this.Margin.Left + this.Margin.Right, this.Margin.Top + this.Margin.Bottom);
        availableSize -= margin;

        if (!float.IsNaN(this.Width))
        {
            availableSize.X = this.Width;
        }

        if (!float.IsNaN(this.Height))
        {
            availableSize.Y = this.Height;
        }

        this.DesiredSize = this.MeasureCore(availableSize) + margin;
    }

    public void Arrange(Rect finalRect)
    {
        float marginW = this.Margin.Left + this.Margin.Right;
        float marginH = this.Margin.Top + this.Margin.Bottom;
        finalRect.X += this.Margin.Left;
        finalRect.Y += this.Margin.Top;
        Vector2 availableSize = finalRect.Size;

        if (!float.IsNaN(this.Width))
        {
            finalRect.Width = this.Width;
        }
        else
        {
            finalRect.Width = MathF.Max(0, finalRect.Width - marginW);
        }

        if (!float.IsNaN(this.Height))
        {
            finalRect.Height = this.Height;
        }
        else
        {
            finalRect.Height = MathF.Max(0, finalRect.Height - marginH);
        }

        var desiredSize = this.DesiredSize;

        finalRect.X += this.HorizontalAlignment switch
        {
            HorizontalAlignment.Left => 0,
            HorizontalAlignment.Stretch => 0,
            HorizontalAlignment.Center => (availableSize.X - desiredSize.X) / 2,
            HorizontalAlignment.Right => availableSize.X - desiredSize.X,
            _ => throw new InvalidOperationException(),
        };

        finalRect.Y += this.VerticalAlignment switch
        {
            VerticalAlignment.Top => 0,
            VerticalAlignment.Stretch => 0,
            VerticalAlignment.Center => (availableSize.Y - desiredSize.Y) / 2,
            VerticalAlignment.Bottom => availableSize.Y - desiredSize.Y,
            _ => throw new InvalidOperationException(),
        };

        var rectPosition = this.ArrangeCore(finalRect);

        this.Position = rectPosition.Position;
        this.ActualSize = rectPosition.Size;
    }

    public void Draw(SpriteBatcher spriteBatcher)
    {
        if (this.Visibility != Visibility.Visible)
        {
            return;
        }

        if (this.BackgroundColor.A > 0)
        {
            spriteBatcher.DrawRectangle(this.Position, this.ActualSize, this.BackgroundColor);
        }

        this.DrawCore(spriteBatcher);
    }

    public virtual void Update(float deltaTime)
    {
        // Do nothing. This should be overridden by subclasses.

        this.Updated?.Invoke(this, deltaTime);
    }

    protected virtual Rect ArrangeCore(Rect finalRect)
    {
        return finalRect;
    }

    protected virtual Vector2 MeasureCore(Vector2 availableSize)
    {
        return availableSize;
    }

    protected virtual void DrawCore(SpriteBatcher spriteBatcher)
    {
        //
    }

    public T? FindByName<T>(string name) where T : Control
    {
        if (this.Name == name)
        {
            return (T)this;
        }

        foreach (var child in this.VisualChildren)
        {
            var result = child.FindByName<T>(name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }


}