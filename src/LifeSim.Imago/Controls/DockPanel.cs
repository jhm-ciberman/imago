using System;
using System.Numerics;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a panel that arranges its child elements along its edges, either horizontally or vertically.
/// </summary>
public class DockPanel : ItemsControl
{
    private bool _lastChildFill = true;

    /// <summary>
    /// Gets or sets a value indicating whether the last child element added to the <see cref="DockPanel"/> fills the remaining available space.
    /// </summary>
    public bool LastChildFill
    {
        get => this._lastChildFill;
        set => this.SetPropertyAndInvalidateArrange(ref this._lastChildFill, value);
    }

    private Thickness _padding = new Thickness(0);

    /// <summary>
    /// Gets or sets the padding of the dock panel, which is the space between the panel's border and its content.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DockPanel"/> class.
    /// </summary>
    public DockPanel()
    {
        //
    }

    /// <inheritdoc/>
    protected override Rect ArrangeOverride(Rect finalRect)
    {
        // Use the "Dock" property to arrange the children.
        // We need to adjust the availableRect in each iteration so we can
        // position the children correctly.
        Rect availableRect = finalRect.Deflate(this.Padding);

        for (var i = 0; i < this.Items.Count; i++)
        {
            var child = this.Items[i];
            bool isLast = i == this.Items.Count - 1;

            if (isLast && this.LastChildFill)
            {
                child.Arrange(availableRect);
                continue;
            }

            Vector2 childDesiredSize = Vector2.Min(child.DesiredSize, availableRect.Size);
            Rect rect;
            switch (child.Dock)
            {
                case Dock.Left:
                    rect = new Rect(availableRect.X, availableRect.Y, childDesiredSize.X, availableRect.Height);
                    child.Arrange(rect);
                    availableRect.X += childDesiredSize.X;
                    availableRect.Width -= childDesiredSize.X;
                    break;
                case Dock.Top:
                    rect = new Rect(availableRect.X, availableRect.Y, availableRect.Width, childDesiredSize.Y);
                    child.Arrange(rect);
                    availableRect.Y += childDesiredSize.Y;
                    availableRect.Height -= childDesiredSize.Y;
                    break;
                case Dock.Right:
                    rect = new Rect(availableRect.Right - childDesiredSize.X, availableRect.Y, childDesiredSize.X, availableRect.Height);
                    child.Arrange(rect);
                    availableRect.Width -= childDesiredSize.X;
                    break;
                case Dock.Bottom:
                    rect = new Rect(availableRect.X, availableRect.Bottom - childDesiredSize.Y, availableRect.Width, childDesiredSize.Y);
                    child.Arrange(rect);
                    availableRect.Height -= childDesiredSize.Y;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        return finalRect;
    }

    /// <inheritdoc/>
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        // Use the "Dock" property to measure the children.
        // We need to adjust the availableSize in each iteration so we can
        // measure the children correctly.
        Rect availableRect = new Rect(Vector2.Zero, availableSize);
        availableRect = availableRect.Deflate(this.Padding);

        float leftRightWidth = 0; // Total width consumed by left/right docked items
        float topBottomHeight = 0; // Total height consumed by top/bottom docked items
        float maxWidthForTopBottom = 0; // Maximum width needed by any top/bottom docked item
        float maxHeightForLeftRight = 0; // Maximum height needed by any left/right docked item

        for (var i = 0; i < this.Items.Count; i++)
        {
            var child = this.Items[i];

            child.Measure(availableRect.Size);
            Vector2 childDesiredSize = child.DesiredSize;

            switch (child.Dock)
            {
                case Dock.Left:
                case Dock.Right:
                    availableRect.Width -= childDesiredSize.X;
                    leftRightWidth += childDesiredSize.X;
                    maxHeightForLeftRight = Math.Max(maxHeightForLeftRight, childDesiredSize.Y);
                    break;
                case Dock.Top:
                case Dock.Bottom:
                    availableRect.Height -= childDesiredSize.Y;
                    topBottomHeight += childDesiredSize.Y;
                    maxWidthForTopBottom = Math.Max(maxWidthForTopBottom, childDesiredSize.X);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        // Calculate the final desired size
        Vector2 desiredSize;
        desiredSize.X = leftRightWidth + maxWidthForTopBottom;
        desiredSize.Y = Math.Max(maxHeightForLeftRight, topBottomHeight);

        return desiredSize + this.Padding.Total;
    }
}
