using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Support;

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
        finalRect = finalRect.Deflate(this.Margin);
        Vector2 availableSize = finalRect.Size;

        var desiredSize = this.DesiredSize - this.Margin.Total;

        switch (this.HorizontalAlignment)
        {
            case HorizontalAlignment.Center:
                finalRect.X += (availableSize.X - desiredSize.X) / 2;
                finalRect.Width = desiredSize.X;
                break;
            case HorizontalAlignment.Right:
                finalRect.X += availableSize.X - desiredSize.X;
                finalRect.Width = desiredSize.X;
                break;
            case HorizontalAlignment.Left:
                finalRect.Width = desiredSize.X;
                break;
            case HorizontalAlignment.Stretch:
                finalRect.Width = availableSize.X;
                break;
            default: throw new InvalidOperationException();
        };

        switch (this.VerticalAlignment)
        {
            case VerticalAlignment.Center:
                finalRect.Y += (availableSize.Y - desiredSize.Y) / 2;
                finalRect.Height = desiredSize.Y;
                break;
            case VerticalAlignment.Bottom:
                finalRect.Y += availableSize.Y - desiredSize.Y;
                finalRect.Height = desiredSize.Y;
                break;
            case VerticalAlignment.Top:
                finalRect.Height = desiredSize.Y;
                break;
            case VerticalAlignment.Stretch:
                finalRect.Height = availableSize.Y;
                break;
            default: throw new InvalidOperationException();
        };

        finalRect.Width = !float.IsNaN(this.Width) ? this.Width : MathF.Max(0, finalRect.Width);
        finalRect.Height = !float.IsNaN(this.Height) ? this.Height : MathF.Max(0, finalRect.Height);

        var rectPosition = this.ArrangeOverride(finalRect);

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
