using System.Numerics;
using Imago.Support.Numerics;

namespace Imago.Controls;

/// <summary>
/// Represents a panel that layers its child elements on top of each other, with each child occupying the same layout slot.
/// </summary>
public class LayeredPanel : ItemsControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LayeredPanel"/> class.
    /// </summary>
    public LayeredPanel()
    {
        //
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    protected override Rect ArrangeOverride(Rect finalRect)
    {
        foreach (var child in this.Items)
        {
            child.Arrange(finalRect);
        }

        return finalRect;
    }
}
