using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Controls;

public abstract class Control
{
    public string Name { get; set; } = string.Empty;

    public Thickness Margin { get; set; } = new Thickness(0);

    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;

    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

    public virtual IEnumerable<Control> VisualChildren { get; } = Enumerable.Empty<Control>();

    public Control()
    {
        //
    }

    public Control(string name)
    {
        this.Name = name;
    }

    public Vector2 Position { get; protected set; } = Vector2.Zero;

    public Vector2 ActualSize { get; protected set; } = Vector2.Zero;

    public Visibility Visibility { get; protected set; } = Visibility.Visible;

    public Vector2 DesiredSize { get; protected set; } = Vector2.Zero;

    public float Width { get; set; } = float.NaN;

    public float Height { get; set; } = float.NaN;

    public void Measure(Vector2 availableSize)
    {
        if (this.Visibility == Visibility.Hidden)
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

        this.MeasureCore(availableSize);
        this.DesiredSize += margin;
    }

    public void Arrange(Rectangle finalRect)
    {
        finalRect.X += this.Margin.Left;
        finalRect.Y += this.Margin.Top;
        finalRect.Width -= this.Margin.Left + this.Margin.Right;
        finalRect.Height -= this.Margin.Top + this.Margin.Bottom;

        var desiredSize = this.DesiredSize;

        finalRect.X += this.HorizontalAlignment switch
        {
            HorizontalAlignment.Left => 0,
            HorizontalAlignment.Stretch => 0,
            HorizontalAlignment.Center => (finalRect.Width - desiredSize.X) / 2,
            HorizontalAlignment.Right => finalRect.Width - desiredSize.X,
            _ => throw new InvalidOperationException(),
        };

        finalRect.Y += this.VerticalAlignment switch
        {
            VerticalAlignment.Top => 0,
            VerticalAlignment.Stretch => 0,
            VerticalAlignment.Center => (finalRect.Height - desiredSize.Y) / 2,
            VerticalAlignment.Bottom => finalRect.Height - desiredSize.Y,
            _ => throw new InvalidOperationException(),
        };

        this.ArrangeCore(finalRect);
    }

    public void Draw(SpriteBatcher spriteBatcher)
    {
        if (this.Visibility == Visibility.Hidden)
        {
            return;
        }

        this.DrawCore(spriteBatcher);
    }

    public virtual void Update(float deltaTime)
    {
        // Do nothing. This should be overridden by subclasses.
    }

    protected virtual void ArrangeCore(Rectangle finalRect)
    {
        this.Position = finalRect.Min;
        this.ActualSize = finalRect.Size;
    }

    protected virtual void MeasureCore(Vector2 availableSize)
    {
        this.DesiredSize = availableSize;
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