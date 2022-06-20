using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Utils;

namespace LifeSim.Engine.Controls;

public class ContentControlBase<TContent> : Control where TContent : Control
{
    /// <summary>
    /// Gets or sets the padding of the content control.
    /// </summary>
    public Thickness Padding { get; set; } = new Thickness(0);

    private TContent? _content;

    /// <summary>
    /// Gets or sets the content of the control.
    /// </summary>
    protected TContent? ContentInternal
    {
        get => this._content;
        set
        {
            if (this._content != value)
            {
                var oldContent = this._content;
                if (oldContent != null)
                {
                    this.RemoveVisualChild(oldContent);
                }

                this._content = value;

                if (this._content != null)
                {
                    this.AddVisualChild(this._content);
                }

                this.InvalidateMeasure();
            }
        }
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        if (this.ContentInternal != null)
        {
            this.ContentInternal.Measure(availableSize);
            return this.ContentInternal.DesiredSize;
        }
        else
        {
            return Vector2.Zero;
        }
    }

    protected override Rect ArrangeCore(Rect finalRect)
    {
        if (this.ContentInternal != null)
        {
            finalRect.X += this.Padding.Left;
            finalRect.Y += this.Padding.Top;
            finalRect.Width -= this.Padding.Horizontal;
            finalRect.Height -= this.Padding.Vertical;

            this.ContentInternal.Arrange(finalRect);
        }

        return finalRect;
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        if (this.ContentInternal != null)
        {
            this.ContentInternal.Draw(spriteBatcher);
        }
    }

    public override void Update(float deltaTime)
    {
        this.ContentInternal?.Update(deltaTime);

        base.Update(deltaTime);
    }
}