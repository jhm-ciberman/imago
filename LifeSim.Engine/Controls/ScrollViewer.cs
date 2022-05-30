using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class ScrollViewer : ContentControl
{
    /// <summary>
    /// Gets or sets if the scroll viewer should scroll horizontally.
    /// </summary>
    public bool HorizontalScrollEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets if the scroll viewer should scroll vertically.
    /// </summary>
    public bool VerticalScrollEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the scroll viewer's scroll offset.
    /// </summary>
    public Vector2 ScrollOffset { get; set; } = Vector2.Zero;

    public ScrollViewer() : base()
    {
        this.ClipToBounds = true;
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        if (this.HorizontalScrollEnabled)
        {
            availableSize.Y = float.PositiveInfinity;
        }

        if (this.VerticalScrollEnabled)
        {
            availableSize.X = float.PositiveInfinity;
        }

        return base.MeasureCore(availableSize);
    }

    protected override Rect ArrangeCore(Rect finalRect)
    {
        if (this.Content != null)
        {
            Vector2 pos = finalRect.Position - this.ScrollOffset;
            Rect rect = new Rect(pos, Vector2.Min(this.Content.DesiredSize, finalRect.Size));
            this.Content.Arrange(rect);
        }

        return finalRect;
    }
}