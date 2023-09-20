using System.Numerics;
using Support;

namespace Imago.Controls;

public class LayeredPanel : ItemsControl
{
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        Vector2 desiredSize = Vector2.Zero;

        foreach (var child in this.Items)
        {
            child.Measure(availableSize);
            desiredSize = Vector2.Max(desiredSize, child.DesiredSize);
        }

        return desiredSize;
    }

    protected override Rect ArrangeOverride(Rect finalRect)
    {
        foreach (var child in this.Items)
        {
            child.Arrange(finalRect);
        }

        return finalRect;
    }
}
