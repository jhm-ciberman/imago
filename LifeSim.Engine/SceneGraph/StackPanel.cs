using System;
using System.Numerics;

namespace LifeSim.Engine.SceneGraph;

public class StackPanel : UIElement
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;

    public StackPanel()
    {
        //
    }

    public override Vector2 Measure(Vector2 availableSize)
    {
        var desiredSize = Vector2.Zero;

        foreach (var child in this.Children)
        {
            if (child.Visible)
            {
                var childDesiredSize = child.Measure(availableSize);

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
        }

        return desiredSize;
    }

    public override void Arrange(Rectangle finalRect)
    {
        // use DesiredSize to calculate the final rect
        // and call Arrange on each child recursively

        var x = finalRect.X;
        var y = finalRect.Y;

        foreach (var child in this.Children)
        {
            if (child.Visible)
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


}