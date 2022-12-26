using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Support;

namespace LifeSim.Engine.Controls;

public class ContentControlBase<TContent> : Control where TContent : Control
{
    private Thickness _padding = new Thickness(0);

    /// <summary>
    /// Gets or sets the padding of the content control.
    /// </summary>
    public Thickness Padding
    {
        get => this._padding;
        set => this.SetPropertyAndInvalidateMeasure(ref this._padding, value);
    }

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

    protected override Vector2 MeasureOverride(Vector2 availableSize)
    {
        if (this.ContentInternal != null)
        {
            availableSize -= this.Padding.Total;
            this.ContentInternal.Measure(availableSize);
            return this.ContentInternal.DesiredSize + this.Padding.Total;
        }
        else
        {
            return Vector2.Zero;
        }
    }

    protected override Rect ArrangeOverride(Rect finalRect)
    {
        if (this.ContentInternal != null)
        {
            Rect contentRect = finalRect.Deflate(this.Padding);
            this.ContentInternal.Arrange(contentRect);
        }

        return finalRect;
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        this.ContentInternal?.Draw(spriteBatcher);
    }

    public override void Update(float deltaTime)
    {
        this.ContentInternal?.Update(deltaTime);

        base.Update(deltaTime);
    }
}
