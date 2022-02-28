using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class DockPanel : ItemsControl
{
    public DockPanel()
    {
        //
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        // Use the "Dock" property to measure the children.
        // The last child is always stretched to fill the remaining space.

        Vector2 orinalAvailableSize = availableSize;

        for (var i = 0; i < this.Items.Count; i++)
        {
            var child = this.Items[i];
            bool isLast = i == this.Items.Count - 1;

            child.Measure(availableSize);

            Vector2 childDesiredSize = child.DesiredSize;

            childDesiredSize = Vector2.Min(childDesiredSize, availableSize);

            if (isLast)
            {
                availableSize.X = 0;
                availableSize.Y = 0;
            }
            else
            {
                switch (child.Dock)
                {
                    case Dock.Left:
                    case Dock.Top:
                    case Dock.Right:
                    case Dock.Bottom:
                        availableSize.X -= childDesiredSize.X;
                        availableSize.Y -= childDesiredSize.Y;
                        break;
                    case Dock.None:
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        return orinalAvailableSize;
    }

    protected override Rect ArrangeCore(Rect finalRect)
    {
        // Use the "Dock" property to arrange the children.
        // The last child is always stretched to fill the remaining space.

        // We need to adjust the availableRect in each iteration so we can
        // position the children correctly.
        Rect availableRect = finalRect;

        for (var i = 0; i < this.Items.Count; i++)
        {
            var child = this.Items[i];
            bool isLast = i == this.Items.Count - 1;

            Vector2 childDesiredSize = child.DesiredSize;

            if (isLast)
            {
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
                    case Dock.None:
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        return finalRect;
    }
}