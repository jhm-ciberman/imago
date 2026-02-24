using System;
using System.Numerics;
using Imago.Support.Numerics;

namespace Imago.Controls;

/// <summary>
/// Represents a panel that positions child elements in sequential order from left to right or top to bottom,
/// automatically wrapping to the next line or column when elements exceed the available space.
/// </summary>
public class WrapPanel : ItemsControl
{
    private Orientation _orientation = Orientation.Horizontal;

    private Thickness _padding = new Thickness(0);

    private Vector2 _gap = Vector2.Zero;

    /// <summary>
    /// Gets or sets the orientation of the wrap panel.
    /// </summary>
    public Orientation Orientation
    {
        get => this._orientation;
        set => this.SetPropertyAndInvalidateArrange(ref this._orientation, value);
    }

    /// <summary>
    /// Gets or sets the padding of the wrap panel.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    /// <summary>
    /// Gets or sets the spacing between child elements, where X is horizontal spacing and Y is vertical spacing.
    /// </summary>
    public Vector2 Gap
    {
        get => this._gap;
        set => this.SetPropertyAndInvalidateMeasure(ref this._gap, value);
    }

    /// <inheritdoc/>
    protected override Vector2 MeasureOverride(Vector2 availableSize)
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
                float horizontalGap = currentLineSize.X > 0 ? this.Gap.X : 0;

                if (currentLineSize.X + horizontalGap + childSize.X > availableSize.X) // Wrap to next row
                {
                    minRequiredSize.X = Math.Max(minRequiredSize.X, currentLineSize.X);
                    minRequiredSize.Y += currentLineSize.Y;
                    if (minRequiredSize.Y > 0)
                    {
                        minRequiredSize.Y += this.Gap.Y;
                    }

                    currentLineSize = childSize;
                }
                else
                {
                    currentLineSize.X += horizontalGap + childSize.X;
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
                float verticalGap = currentLineSize.Y > 0 ? this.Gap.Y : 0;

                if (currentLineSize.Y + verticalGap + childSize.Y > availableSize.Y) // Wrap to next column
                {
                    minRequiredSize.X += currentLineSize.X;
                    if (minRequiredSize.X > 0)
                    {
                        minRequiredSize.X += this.Gap.X;
                    }

                    minRequiredSize.Y = Math.Max(minRequiredSize.Y, currentLineSize.Y);
                    currentLineSize = childSize;
                }
                else
                {
                    currentLineSize.X = Math.Max(currentLineSize.X, childSize.X);
                    currentLineSize.Y += verticalGap + childSize.Y;
                }
            }

            minRequiredSize.X += currentLineSize.X;
            minRequiredSize.Y = Math.Max(minRequiredSize.Y, currentLineSize.Y);
        }

        return minRequiredSize + this.Padding.Total;
    }

    /// <inheritdoc/>
    protected override Rect ArrangeOverride(Rect finalRect)
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
                float horizontalGap = currentLineSize.X > 0 ? this.Gap.X : 0;

                if (currentLineSize.X + horizontalGap + childSize.X > availableSize.X)
                {
                    currentPos.Y += currentLineSize.Y + this.Gap.Y;
                    currentPos.X = finalRect.Position.X;
                    currentLineSize = childSize;
                }
                else
                {
                    currentLineSize.X += horizontalGap + childSize.X;
                    currentLineSize.Y = Math.Max(currentLineSize.Y, childSize.Y);
                }

                child.Arrange(new Rect(currentPos, childSize));
                currentPos.X += childSize.X + this.Gap.X;
            }
        }
        else
        {
            foreach (var child in this.Items)
            {
                var childSize = child.DesiredSize;
                float verticalGap = currentLineSize.Y > 0 ? this.Gap.Y : 0;

                if (currentLineSize.Y + verticalGap + childSize.Y > availableSize.Y)
                {
                    currentPos.X += currentLineSize.X + this.Gap.X;
                    currentPos.Y = finalRect.Position.Y;
                    currentLineSize = childSize;
                }
                else
                {
                    currentLineSize.X = Math.Max(currentLineSize.X, childSize.X);
                    currentLineSize.Y += verticalGap + childSize.Y;
                }

                child.Arrange(new Rect(currentPos, childSize));
                currentPos.Y += childSize.Y + this.Gap.Y;
            }
        }

        return originalRect;
    }
}
