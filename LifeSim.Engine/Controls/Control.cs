using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.SceneGraph;

namespace LifeSim.Engine.Controls;

public class Control : Visual
{
    /// <summary>
    /// Raised when the control is updated.
    /// </summary>
    public event EventHandler<float>? Updated;

    /// <summary>
    /// Gets or sets the margin of the control.
    /// </summary>
    public Thickness Margin { get; set; } = new Thickness(0);

    /// <summary>
    /// Gets or sets the horizontal alignment of the control.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;

    /// <summary>
    /// Gets or sets the vertical alignment of the control.
    /// </summary>
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

    /// <summary>
    /// Gets or sets the dock position of the control.
    /// </summary>
    public Dock Dock { get; set; } = Dock.Left;

    /// <summary>
    /// Gets or sets the background color of the control.
    /// </summary>
    public Color Background { get; set; } = Color.Transparent;

    /// <summary>
    /// Gets the parent of the control or null if the control has no parent.
    /// </summary>
    public Control? Parent { get; internal set; }


    /// <summary>
    /// Gets the position of the control.
    /// </summary>
    public Vector2 Position { get; protected set; } = Vector2.Zero;

    /// <summary>
    /// Gets the actual size of the control. That is, the real size the control takes up.
    /// </summary>
    public Vector2 ActualSize { get; protected set; } = Vector2.Zero;

    /// <summary>
    /// Gets the desired size of the control. That is the size that the control has requested to take up after the measure pass.
    /// </summary>
    public Vector2 DesiredSize { get; protected set; } = Vector2.Zero;

    /// <summary>
    /// Gets or sets the width of the control. A value of float.NaN indicates that the width should be calculated automatically using the control's content.
    /// </summary>
    public float Width { get; set; } = float.NaN;

    /// <summary>
    /// Gets or sets the height of the control. A value of float.NaN indicates that the height should be calculated automatically using the control's content.
    /// </summary>
    public float Height { get; set; } = float.NaN;

    /// <summary>
    /// Gets or sets the style of the control.
    /// </summary>
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

    /// <summary>
    /// Creates a new instance of the <see cref="Control"/> class.
    /// </summary>
    public Control()
    {
        this.Style = Style.GetDefaultStyle(this.GetType());
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Control"/> class.
    /// </summary>
    /// <param name="name">The name of the control.</param>
    public Control(string name) : this()
    {
        this.Name = name;
    }

    /// <summary>
    /// Performs the measure pass of the layout process. In the measure pass, the control computes the desired size of the control
    /// and updates the <see cref="DesiredSize"/> property.
    /// </summary>
    /// <param name="availableSize">The available size that this object can give to child objects. Infinity can be specified as a value to indicate that the object will size to whatever content is available.</param>
    public void Measure(Vector2 availableSize)
    {
        if (this.Visibility == Visibility.Collapsed)
        {
            this.DesiredSize = Vector2.Zero;
            return;
        }

        var margin = this.Margin.Total;
        availableSize -= margin;

        Vector2 desiredSize = this.MeasureCore(availableSize);

        if (!float.IsNaN(this.Width))
        {
            desiredSize.X = this.Width;
        }

        if (!float.IsNaN(this.Height))
        {
            desiredSize.Y = this.Height;
        }

        this.DesiredSize = desiredSize + margin;
    }

    /// <summary>
    /// Performs the arrange pass of the layout process. In the arrange pass, the control positions its children and computes the actual size of the control.
    /// </summary>
    /// <param name="finalSize">The final size that this object should use to arrange itself and its children.</param>
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

    protected override Rect GetBounds()
    {
        return new Rect(this.Position, this.ActualSize);
    }

    /// <summary>
    /// Updates the control. This method is called by the <see cref="Root"/> of the control in each frame.
    /// </summary>
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

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        if (this.Background.A > 0)
        {
            spriteBatcher.DrawRectangle(this.Position, this.ActualSize, this.Background);
        }
    }
}