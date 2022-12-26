using System;
using System.Numerics;
using LifeSim.Support;

namespace LifeSim.Engine.Controls;

public class WrapPanel : ItemsControl
{
    private Orientation _orientation = Orientation.Horizontal;

    /// <summary>
    /// Gets or sets the orientation of the wrap panel.
    /// </summary>
    public Orientation Orientation
    {
        get => this._orientation;
        set => this.SetPropertyAndInvalidateArrange(ref this._orientation, value);
    }

    private Thickness _padding = new Thickness(0);

    /// <summary>
    /// Gets or sets the padding of the wrap panel.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        Vector2 minRequiredSize = Vector2.Zero;
        Vector2 currentLineSize = Vector2.Zero;
        availableSize -= this.Padding.Total;

        if (this.Orientation == Orientation.Horizontal)
        {
            foreach (var child in this.Items)
            {
                child.Measure(availableSize);
                var childSize = child.DesiredSize;
                if (currentLineSize.X + childSize.X > availableSize.X)
                {
                    minRequiredSize.X = Math.Max(minRequiredSize.X, currentLineSize.X);
                    minRequiredSize.Y += currentLineSize.Y;
                    currentLineSize = childSize;
                }
                else
                {
                    currentLineSize.X += childSize.X;
                    currentLineSize.Y = Math.Max(currentLineSize.Y, childSize.Y);
                }
            }

            minRequiredSize.X = Math.Max(minRequiredSize.X, currentLineSize.X);
            minRequiredSize.Y += currentLineSize.Y;
        }
        else
        {
            foreach (var child in this.Items)
            {
                child.Measure(availableSize);
                var childSize = child.DesiredSize;
                if (currentLineSize.Y + childSize.Y > availableSize.Y)
                {
                    minRequiredSize.X += currentLineSize.X;
                    minRequiredSize.Y = Math.Max(minRequiredSize.Y, currentLineSize.Y);
                    currentLineSize = childSize;
                }
                else
                {
                    currentLineSize.X = Math.Max(currentLineSize.X, childSize.X);
                    currentLineSize.Y += childSize.Y;
                }
            }

            minRequiredSize.X += currentLineSize.X;
            minRequiredSize.Y = Math.Max(minRequiredSize.Y, currentLineSize.Y);
        }

        return minRequiredSize + this.Padding.Total;
    }

    protected override Rect ArrangeCore(Rect finalRect)
    {
        Rect originalRect = finalRect;
        finalRect = finalRect.Deflate(this.Padding);
        Vector2 availableSize = finalRect.Size;
        Vector2 currentLineSize = Vector2.Zero;
        Vector2 currentPos = finalRect.Position;

        if (this.Orientation == Orientation.Horizontal)
        {
            foreach (var child in this.Items)
            {
                var childSize = child.DesiredSize;
                if (currentLineSize.X + childSize.X > availableSize.X)
                {
                    currentPos.Y += currentLineSize.Y;
                    currentPos.X = finalRect.Position.X;
                    currentLineSize = childSize;
                }
                else
                {
                    currentLineSize.X += childSize.X;
                    currentLineSize.Y = Math.Max(currentLineSize.Y, childSize.Y);
                }

                child.Arrange(new Rect(currentPos, childSize));
                currentPos.X += childSize.X;
            }
        }
        else
        {
            foreach (var child in this.Items)
            {
                var childSize = child.DesiredSize;
                if (currentLineSize.Y + childSize.Y > availableSize.Y)
                {
                    currentPos.X += currentLineSize.X;
                    currentPos.Y = finalRect.Position.Y;
                    currentLineSize = childSize;
                }
                else
                {
                    currentLineSize.X = Math.Max(currentLineSize.X, childSize.X);
                    currentLineSize.Y += childSize.Y;
                }

                child.Arrange(new Rect(currentPos, childSize));
                currentPos.Y += childSize.Y;
            }
        }

        return originalRect;
    }
}
