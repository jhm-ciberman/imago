using System;
using System.Numerics;
using LifeSim.Utils;

namespace LifeSim.Engine.Controls;

public class StackPanel : ItemsControl
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;

    public Thickness Padding { get; set; } = new Thickness(0);

    public StackPanel()
    {
        //
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
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

    protected override Rect ArrangeCore(Rect finalRect)
    {
        // use DesiredSize to calculate the final rect
        // and call Arrange on each child recursively

        Rect innerRect = finalRect;
        innerRect.X += this.Padding.Left;
        innerRect.Y += this.Padding.Top;
        innerRect.Width -= this.Padding.Horizontal;
        innerRect.Height -= this.Padding.Vertical;
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

            return new Rect(finalRect.X, finalRect.Y, x - finalRect.X + this.Padding.Horizontal, finalRect.Height);
        }
        else
        {
            foreach (var child in this.Items)
            {
                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(x, y, innerRect.Width, childDesiredSize.Y));
                y += childDesiredSize.Y;
            }

            return new Rect(finalRect.X, finalRect.Y, finalRect.Width, y - innerRect.Y + this.Padding.Vertical);
        }
    }


}