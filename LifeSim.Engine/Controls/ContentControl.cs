using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class ContentControl : Control
{
    private IEnumerable<Control> _visualChildren = Array.Empty<Control>();
    public override IEnumerable<Control> VisualChildren => this._visualChildren;

    private Control? _content;
    public Control? Content
    {
        get => this._content;
        set
        {
            if (this._content != value)
            {
                if (this._content != null)
                {
                    this._content.Parent = null;
                    this._content.Root = null;
                }

                this._content = value;

                if (this._content != null)
                {
                    this._content.Parent = this;
                    this._content.Root = this.Root;
                    this._visualChildren = new[] { this._content };
                }
                else
                {
                    this._visualChildren = Array.Empty<Control>();
                }
            }
        }
    }


    /// <summary>
    /// Gets or sets whether the content is clipped to the control's bounds.
    /// </summary>
    public bool ClipToBounds { get; set; } = false;


    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        if (this.Content != null)
        {
            this.Content.Measure(availableSize);
            return availableSize;
        }
        else
        {
            return availableSize;
        }
    }

    protected override Rect ArrangeCore(Rect finalRect)
    {
        if (this.Content != null)
        {
            this.Content.Arrange(finalRect);
        }

        return finalRect;
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        if (this.Content != null)
        {
            if (this.ClipToBounds)
            {
                Rect rect = new Rect(this.Position * this.Root!.Zoom, this.ActualSize * this.Root!.Zoom);
                spriteBatcher.BeginClipRectangle(rect);
                this.Content.Draw(spriteBatcher);
                spriteBatcher.EndClipRectangle();
            }
            else
            {
                this.Content.Draw(spriteBatcher);
            }
        }
    }
}