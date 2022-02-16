using System;
using System.Numerics;

namespace LifeSim.Engine.Controls;

public class StackPanel : ItemsControl
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;

    public StackPanel()
    {
        //
    }

    protected override void MeasureCore(Vector2 availableSize)
    {
        var desiredSize = Vector2.Zero;

        foreach (var child in this.Items)
        {
            child.Measure(availableSize);

            var childDesiredSize = child.DesiredSize;

            if (this.Orientation == Orientation.Horizontal)
            {
                desiredSize.X += childDesiredSize.X;
                desiredSize.Y = Math.Max(desiredSize.Y, childDesiredSize.Y);
            }
            else
            {
                desiredSize.X = Math.Max(desiredSize.X, childDesiredSize.X);
                desiredSize.Y += childDesiredSize.Y;
            }
        }

        this.DesiredSize = desiredSize;
    }

    protected override void ArrangeCore(Rectangle finalRect)
    {
        // use DesiredSize to calculate the final rect
        // and call Arrange on each child recursively

        var x = finalRect.X;
        var y = finalRect.Y;

        foreach (var child in this.Items)
        {
            var childDesiredSize = child.DesiredSize;

            if (this.Orientation == Orientation.Horizontal)
            {
                child.Arrange(new Rectangle(x, y, childDesiredSize.X, finalRect.Height));
                x += childDesiredSize.X;
            }
            else
            {
                child.Arrange(new Rectangle(x, y, finalRect.Width, childDesiredSize.Y));
                y += childDesiredSize.Y;
            }
        }
    }


}