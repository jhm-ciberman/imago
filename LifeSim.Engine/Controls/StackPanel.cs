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

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        var desiredSize = Vector2.Zero;

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

        return desiredSize;
    }

    protected override Rect ArrangeCore(Rect finalRect)
    {
        // use DesiredSize to calculate the final rect
        // and call Arrange on each child recursively

        var x = finalRect.X;
        var y = finalRect.Y;

        if (this.Orientation == Orientation.Horizontal)
        {
            foreach (var child in this.Items)
            {
                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(x, y, childDesiredSize.X, finalRect.Height));
                x += childDesiredSize.X;
            }

            return new Rect(finalRect.X, finalRect.Y, x - finalRect.X, finalRect.Height);
        }
        else
        {
            foreach (var child in this.Items)
            {
                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(x, y, finalRect.Width, childDesiredSize.Y));
                y += childDesiredSize.Y;
            }

            return new Rect(finalRect.X, finalRect.Y, finalRect.Width, y - finalRect.Y);
        }
    }


}