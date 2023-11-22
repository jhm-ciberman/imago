using System;
using System.Numerics;
using Support;

namespace Imago.Controls;

public class StackPanel : ItemsControl
{
    private Orientation _orientation = Orientation.Vertical;

    private Thickness _padding = new Thickness(0);

    /// <summary>
    /// Gets or sets the orientation of the stack panel.
    /// </summary>
    public Orientation Orientation
    {
        get => this._orientation;
        set => this.SetPropertyAndInvalidateMeasure(ref this._orientation, value);
    }

    /// <summary>
    /// Gets or sets the padding of the stack panel.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        var desiredSize = Vector2.Zero;

        availableSize -= this.Padding.Total;

        if (this.Orientation == Orientation.Horizontal)
        {
            foreach (var child in this.Items)
            {
                child.Measure(availableSize);
                var childDesiredSize = child.DesiredSize;
                desiredSize.X += childDesiredSize.X;
                desiredSize.Y = Math.Max(desiredSize.Y, childDesiredSize.Y);
            }
        }
        else
        {
            foreach (var child in this.Items)
            {
                child.Measure(availableSize);
                var childDesiredSize = child.DesiredSize;
                desiredSize.X = Math.Max(desiredSize.X, childDesiredSize.X);
                desiredSize.Y += childDesiredSize.Y;
            }
        }

        return desiredSize + this.Padding.Total;
    }

    protected override Rect ArrangeOverride(Rect finalRect)
    {
        // use DesiredSize to calculate the final rect
        // and call Arrange on each child recursively

        Rect innerRect = finalRect.Deflate(this.Padding);
        var x = innerRect.X;
        var y = innerRect.Y;

        if (this.Orientation == Orientation.Horizontal)
        {
            foreach (var child in this.Items)
            {
                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(x, y, childDesiredSize.X, innerRect.Height));
                x += childDesiredSize.X;
            }
        }
        else
        {
            foreach (var child in this.Items)
            {
                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(x, y, innerRect.Width, childDesiredSize.Y));
                y += childDesiredSize.Y;
            }
        }

        return finalRect;
    }


}
