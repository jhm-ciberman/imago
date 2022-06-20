using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Utils;

namespace LifeSim.Engine.Controls;

public class Control : Visual
{
    private Thickness _margin = new Thickness(0);

    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;

    private VerticalAlignment _verticalAlignment = VerticalAlignment.Top;

    /// <summary>
    /// Gets or sets the margin of the control.
    /// </summary>
    public Thickness Margin
    {
        get => this._margin;
        set
        {
            if (this._margin != value)
            {
                this._margin = value;
                this.InvalidateMeasure();
            }
        }
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the control.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => this._horizontalAlignment;
        set
        {
            if (this._horizontalAlignment != value)
            {
                this._horizontalAlignment = value;
                this.InvalidateArrange();
            }
        }
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the control.
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => this._verticalAlignment;
        set
        {
            if (this._verticalAlignment != value)
            {
                this._verticalAlignment = value;
                this.InvalidateArrange();
            }
        }
    }

    private Dock _dock = Dock.Left;

    /// <summary>
    /// Gets or sets the dock position of the control.
    /// </summary>
    public Dock Dock
    {
        get => this._dock;
        set
        {
            if (this._dock != value)
            {
                this._dock = value;
                this.InvalidateArrange();
            }
        }
    }

    private IBrush? _background = null;

    /// <summary>
    /// Gets or sets the background brush of the control.
    /// </summary>
    public IBrush? Background
    {
        get => this._background;
        set => this.SetBrushImpl(value);
    }

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

    private float _width = float.NaN;
    private float _height = float.NaN;

    /// <summary>
    /// Gets or sets the width of the control. A value of float.NaN indicates that the width should be calculated automatically using the control's content.
    /// </summary>
    public float Width
    {
        get => this._width;
        set
        {
            if (this._width != value)
            {
                this._width = value;
                this.InvalidateMeasure();
            }
        }
    }

    /// <summary>
    /// Gets or sets the height of the control. A value of float.NaN indicates that the height should be calculated automatically using the control's content.
    /// </summary>
    public float Height
    {
        get => this._height;
        set
        {
            if (this._height != value)
            {
                this._height = value;
                this.InvalidateMeasure();
            }
        }
    }

    /// <summary>
    /// Gets whether the measure pass is valid.
    /// </summary>
    public bool IsMeasureValid { get; private set; } = false;

    /// <summary>
    /// Gets whether the arrange pass is valid.
    /// </summary>
    public bool IsArrangeValid { get; private set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="Control"/> class.
    /// </summary>
    public Control() : base() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Control"/> class.
    /// </summary>
    /// <param name="style">The style of the control.</param>
    public Control(Style? style) : base(style) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Control"/> class.
    /// </summary>
    /// <param name="name">The name of the control.</param>
    /// <param name="style">The style of the control.</param>
    public Control(string name, Style? style = null) : base(name, style) { }

    /// <summary>
    /// Performs the measure pass of the layout process. In the measure pass, the control computes the desired size of the control
    /// and updates the <see cref="DesiredSize"/> property.
    /// </summary>
    /// <param name="availableSize">The available size that this object can give to child objects. Infinity can be specified as a value to indicate that the object will size to whatever content is available.</param>
    public void Measure(Vector2 availableSize)
    {
        if (this.IsMeasureValid) return;
        this.IsMeasureValid = true;

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
        if (this.IsArrangeValid) return;
        this.IsArrangeValid = true;

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

        switch (this.HorizontalAlignment)
        {
            case HorizontalAlignment.Center:
                finalRect.X += (availableSize.X - desiredSize.X) / 2;
                break;
            case HorizontalAlignment.Right:
                finalRect.X += availableSize.X - desiredSize.X;
                break;
        };

        switch (this.VerticalAlignment)
        {
            case VerticalAlignment.Center:
                finalRect.Y += (availableSize.Y - desiredSize.Y) / 2;
                break;
            case VerticalAlignment.Bottom:
                finalRect.Y += availableSize.Y - desiredSize.Y;
                break;
        };

        var rectPosition = this.ArrangeCore(finalRect);

        if (this.HorizontalAlignment == HorizontalAlignment.Stretch)
        {
            rectPosition.Width = finalRect.Width;
        }

        if (this.VerticalAlignment == VerticalAlignment.Stretch)
        {
            rectPosition.Height = finalRect.Height;
        }

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
        this.Background?.DrawRectangle(spriteBatcher, this.Position, this.ActualSize);
    }

    protected internal void InvalidateMeasure()
    {
        this.ForceMeasure();
        this.PropagateInvalidMeasureToChildren();
    }

    private void PropagateInvalidMeasureToChildren()
    {
        foreach (var child in this.VisualChildren)
        {
            if (child is not Control childControl) continue;
            if (!childControl.IsMeasureValid) continue;

            childControl.IsMeasureValid = false;
            childControl.IsArrangeValid = false;
            childControl.PropagateInvalidMeasureToChildren();
        }
    }

    private void ForceMeasure()
    {
        if (!this.IsMeasureValid) return;

        this.IsMeasureValid = false;
        this.IsArrangeValid = false;

        this.Parent?.ForceMeasure();
    }

    protected internal void InvalidateArrange()
    {
        this.ForceArrange();
        this.PropagateInvalidArrangeToChildren();
    }

    private void PropagateInvalidArrangeToChildren()
    {
        foreach (var child in this.VisualChildren)
        {
            if (child is not Control childControl) continue;
            if (!childControl.IsArrangeValid) continue;

            childControl.IsArrangeValid = false;
            childControl.PropagateInvalidArrangeToChildren();
        }
    }

    private void ForceArrange()
    {
        if (!this.IsArrangeValid) return;

        this.IsArrangeValid = false;

        this.Parent?.ForceArrange();
    }

    public override void OnAddedToVisualTree(UIPage page)
    {
        this.IsArrangeValid = false;
        this.IsMeasureValid = false;

        base.OnAddedToVisualTree(page);

        if (this.Background is IAnimatedBrush animatedBrush)
        {
            page.AddAnimatedBrush(animatedBrush);
        }
    }

    public override void OnRemovedFromVisualTree(UIPage page)
    {
        base.OnRemovedFromVisualTree(page);

        if (this.Background is IAnimatedBrush animatedBrush)
        {
            page.RemoveAnimatedBrush(animatedBrush);
        }
    }

    protected void SetBrushImpl(IBrush? brush)
    {
        if (this._background == brush) return;

        if (this.Root != null && this._background is IAnimatedBrush animatedBrush)
        {
            this.Root.RemoveAnimatedBrush(animatedBrush);
        }

        this._background = brush;

        if (this.Root != null && this._background is IAnimatedBrush animatedBrush2)
        {
            this.Root.AddAnimatedBrush(animatedBrush2);
        }
    }

    public void InvalidateArrangeRecursive()
    {
        this.InvalidateArrange();
        foreach (var child in this.VisualChildren)
        {
            if (child is not Control childControl) continue;
            childControl.InvalidateArrangeRecursive();
        }
    }

    public void InvalidateMeasureRecursive()
    {
        this.InvalidateMeasure();
        foreach (var child in this.VisualChildren)
        {
            if (child is not Control childControl) continue;
            childControl.InvalidateMeasureRecursive();
        }
    }

    protected override void AddVisualChild(Visual child)
    {
        base.AddVisualChild(child);
        if (child is not Control control) return;
        control.Parent = this;
        control.InvalidateMeasure();
    }

    protected override void RemoveVisualChild(Visual child)
    {
        base.RemoveVisualChild(child);
        if (child is not Control) return;
        this.InvalidateMeasure();
    }
}