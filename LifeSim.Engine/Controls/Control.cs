using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Utils;

namespace LifeSim.Engine.Controls;

public class Control : Visual
{
    private Thickness _margin = new Thickness(0);
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Top;
    private Dock _dock = Dock.Left;
    private IBrush? _background = null;
    private float _width = float.NaN;
    private float _height = float.NaN;

    /// <summary>
    /// Gets or sets the margin of the control.
    /// </summary>
    public Thickness Margin
    {
        get => this._margin;
        set => this.SetPropertyAndInvalidateMeasure(ref this._margin, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the control.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => this._horizontalAlignment;
        set => this.SetPropertyAndInvalidateArrange(ref this._horizontalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the vertical alignment of the control.
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => this._verticalAlignment;
        set => this.SetPropertyAndInvalidateArrange(ref this._verticalAlignment, value);
    }

    /// <summary>
    /// Gets or sets the dock position of the control.
    /// </summary>
    public Dock Dock
    {
        get => this._dock;
        set => this.SetPropertyAndInvalidateArrange(ref this._dock, value);
    }

    /// <summary>
    /// Gets or sets the background brush of the control.
    /// </summary>
    public IBrush? Background
    {
        get => this._background;
        set => this.SetBrushImpl(value);
    }

    /// <summary>
    /// Gets or sets the width of the control. A value of float.NaN indicates that the width should be calculated automatically using the control's content.
    /// </summary>
    public float Width
    {
        get => this._width;
        set => this.SetPropertyAndInvalidateMeasure(ref this._width, value);
    }

    /// <summary>
    /// Gets or sets the height of the control. A value of float.NaN indicates that the height should be calculated automatically using the control's content.
    /// </summary>
    public float Height
    {
        get => this._height;
        set => this.SetPropertyAndInvalidateMeasure(ref this._height, value);
    }



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

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        var margin = this.Margin.Total;
        availableSize -= margin;

        Vector2 desiredSize = this.MeasureOverride(availableSize);

        if (!float.IsNaN(this.Width))
        {
            desiredSize.X = this.Width;
        }

        if (!float.IsNaN(this.Height))
        {
            desiredSize.Y = this.Height;
        }

        return desiredSize + margin;
    }

    protected override Rect ArrangeCore(Rect finalRect)
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

        var rectPosition = this.ArrangeOverride(finalRect);

        if (this.HorizontalAlignment == HorizontalAlignment.Stretch)
        {
            rectPosition.Width = finalRect.Width;
        }

        if (this.VerticalAlignment == VerticalAlignment.Stretch)
        {
            rectPosition.Height = finalRect.Height;
        }

        return rectPosition;
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

    protected virtual Rect ArrangeOverride(Rect finalRect)
    {
        return finalRect;
    }

    protected virtual Vector2 MeasureOverride(Vector2 availableSize)
    {
        return availableSize;
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        this.Background?.DrawRectangle(spriteBatcher, this.Position, this.ActualSize);
    }


    public override void OnAddedToVisualTree(UIPage page)
    {
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
}