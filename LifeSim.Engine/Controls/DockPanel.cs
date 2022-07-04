using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Utils;

namespace LifeSim.Engine.Controls;

public class DockPanel : ItemsControl
{
    private bool _lastChildFill = true;

    /// <summary>
    /// Gets or sets whether the last child is stretched to fill the remaining space.
    /// </summary>
    public bool LastChildFill
    {
        get => this._lastChildFill;
        set => this.SetPropertyAndInvalidateArrange(ref this._lastChildFill, value);
    }

    private Thickness _padding = new Thickness(0);

    /// <summary>
    /// Gets or sets the padding of the dock panel.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    public DockPanel()
    {
        //
    }

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

            Vector2 childDesiredSize = Vector2.Min(child.DesiredSize, availableRect.Size);

            if (isLast)
            {
                if (!this.LastChildFill)
                {
                    availableRect.Size = childDesiredSize;
                }

                child.Arrange(availableRect);
            }
            else
            {
                switch (child.Dock)
                {
                    case Dock.Left:
                        child.Arrange(new Rect(availableRect.X, availableRect.Y, childDesiredSize.X, availableRect.Height));
                        availableRect.X += childDesiredSize.X;
                        availableRect.Width -= childDesiredSize.X;
                        break;
                    case Dock.Top:
                        child.Arrange(new Rect(availableRect.X, availableRect.Y, availableRect.Width, childDesiredSize.Y));
                        availableRect.Y += childDesiredSize.Y;
                        availableRect.Height -= childDesiredSize.Y;
                        break;
                    case Dock.Right:
                        child.Arrange(new Rect(availableRect.Right - childDesiredSize.X, availableRect.Y, childDesiredSize.X, availableRect.Height));
                        availableRect.Width -= childDesiredSize.X;
                        break;
                    case Dock.Bottom:
                        child.Arrange(new Rect(availableRect.X, availableRect.Bottom - childDesiredSize.Y, availableRect.Width, childDesiredSize.Y));
                        availableRect.Height -= childDesiredSize.Y;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }

        }

        return finalRect;
    }

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        // Use the "Dock" property to measure the children.
        // We need to adjust the availableSize in each iteration so we can
        // measure the children correctly.
        Rect availableRect = new Rect(Vector2.Zero, availableSize);
        availableRect = availableRect.Deflate(this.Padding);
        Vector2 desiredSize = Vector2.Zero;

        for (var i = 0; i < this.Items.Count; i++)
        {
            var child = this.Items[i];

            child.Measure(availableRect.Size);
            Vector2 childDesiredSize = child.DesiredSize;
            desiredSize = Vector2.Max(desiredSize, childDesiredSize + availableRect.Position);

            switch (child.Dock)
            {
                case Dock.Left:
                    availableRect.X += childDesiredSize.X;
                    availableRect.Width -= childDesiredSize.X;
                    break;
                case Dock.Top:
                    availableRect.Y += childDesiredSize.Y;
                    availableRect.Height -= childDesiredSize.Y;
                    break;
                case Dock.Right:
                    availableRect.X += childDesiredSize.X;
                    availableRect.Width -= childDesiredSize.X;
                    break;
                case Dock.Bottom:
                    availableRect.Y += childDesiredSize.Y;
                    availableRect.Height -= childDesiredSize.Y;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        return desiredSize + this.Padding.Total;
    }
}