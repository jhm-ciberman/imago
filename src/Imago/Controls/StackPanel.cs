using System;
using System.Numerics;
using Imago.Support.Numerics;

namespace Imago.Controls;

/// <summary>
/// Represents a panel that arranges child elements in a single line, either horizontally or vertically.
/// </summary>
public class StackPanel : ItemsControl
{
    private Orientation _orientation = Orientation.Vertical;

    private Thickness _padding = new Thickness(0);

    private float _gap = 0f;

    /// <summary>
    /// Gets or sets the orientation of the stack panel, which determines whether children are arranged horizontally or vertically.
    /// </summary>
    public Orientation Orientation
    {
        get => this._orientation;
        set => this.SetPropertyAndInvalidateMeasure(ref this._orientation, value);
    }

    /// <summary>
    /// Gets or sets the padding of the stack panel, which is the space between the panel's border and its content.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

    /// <summary>
    /// Gets or sets the spacing between child elements.
    /// </summary>
    public float Gap
    {
        get => this._gap;
        set => this.SetPropertyAndInvalidateMeasure(ref this._gap, value);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StackPanel"/> class.
    /// </summary>
    public StackPanel()
    {
        //
    }

    /// <inheritdoc/>
    protected override Vector2 MeasureOverride(Vector2 availableSize)
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

            if (this.Items.Count > 1)
            {
                desiredSize.X += (this.Items.Count - 1) * this.Gap;
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

            if (this.Items.Count > 1)
            {
                desiredSize.Y += (this.Items.Count - 1) * this.Gap;
            }
        }

        return desiredSize + this.Padding.Total;
    }

    /// <inheritdoc/>
    protected override Rect ArrangeOverride(Rect finalRect)
    {
        Rect innerRect = finalRect.Deflate(this.Padding);
        var x = innerRect.X;
        var y = innerRect.Y;

        if (this.Orientation == Orientation.Horizontal)
        {
            foreach (var child in this.Items)
            {
                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(x, y, childDesiredSize.X, innerRect.Height));
                x += childDesiredSize.X + this.Gap;
            }
        }
        else
        {
            foreach (var child in this.Items)
            {
                var childDesiredSize = child.DesiredSize;
                child.Arrange(new Rect(x, y, innerRect.Width, childDesiredSize.Y));
                y += childDesiredSize.Y + this.Gap;
            }
        }

        return finalRect;
    }
}
