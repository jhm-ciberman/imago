using System;
using System.Numerics;
using LifeSim.Support.Numerics;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a layout panel that allows absolute positioning of its child elements.
/// </summary>
public class Canvas : ItemsControl
{
    /// <inheritdoc/>
    protected override Rect ArrangeOverride(Rect finalRect)
    {
        foreach (Control child in this.Items)
        {
            float left = child.Left;
            float top = child.Top;
            float right = child.Right;
            float bottom = child.Bottom;

            if (!float.IsNaN(left) && !float.IsNaN(right))
            {
                throw new InvalidOperationException("Cannot set both Left and Right on a child element.");
            }

            if (!float.IsNaN(top) && !float.IsNaN(bottom))
            {
                throw new InvalidOperationException("Cannot set both Top and Bottom on a child element.");
            }

            Rect childRect = new Rect(Vector2.Zero, child.DesiredSize);

            if (!float.IsNaN(left))
            {
                childRect.X = left;
            }
            else if (!float.IsNaN(right))
            {
                childRect.X = finalRect.Width - right - child.DesiredSize.X;
            }
            else
            {
                childRect.X = (finalRect.Width - child.DesiredSize.X) / 2.0f;
            }

            if (!float.IsNaN(top))
            {
                childRect.Y = top;
            }
            else if (!float.IsNaN(bottom))
            {
                childRect.Y = finalRect.Height - bottom - child.DesiredSize.Y;
            }
            else
            {
                childRect.Y = (finalRect.Height - child.DesiredSize.Y) / 2.0f;
            }

            childRect.Position += finalRect.Position;
            child.Arrange(childRect);
        }

        return finalRect;
    }

    /// <summary>
    /// Measures the size required for the canvas and its children.
    /// </summary>
    /// <param name="availableSize">The available size that this element can give to child elements.</param>
    /// <returns>The size that this element determines it needs during layout, based on its calculations of child element sizes.</returns>
    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        // Measure all child elements.
        Vector2 desiredSize = Vector2.Zero;

        foreach (var child in this.Items)
        {
            child.Measure(availableSize);

            desiredSize = Vector2.Max(desiredSize, child.DesiredSize);

            float left = child.Left;
            float top = child.Top;
            float right = child.Right;
            float bottom = child.Bottom;

            if (!float.IsNaN(left) && !float.IsNaN(right))
            {
                throw new InvalidOperationException("Cannot set both Left and Right on a child element.");
            }

            if (!float.IsNaN(top) && !float.IsNaN(bottom))
            {
                throw new InvalidOperationException("Cannot set both Top and Bottom on a child element.");
            }

            if (!float.IsNaN(left))
            {
                desiredSize.X = Math.Max(desiredSize.X, left + child.DesiredSize.X);
            }

            if (!float.IsNaN(right))
            {
                desiredSize.X = Math.Max(desiredSize.X, right + child.DesiredSize.X);
            }

            if (!float.IsNaN(top))
            {
                desiredSize.Y = Math.Max(desiredSize.Y, top + child.DesiredSize.Y);
            }

            if (!float.IsNaN(bottom))
            {
                desiredSize.Y = Math.Max(desiredSize.Y, bottom + child.DesiredSize.Y);
            }
        }


        return desiredSize;
    }
}
