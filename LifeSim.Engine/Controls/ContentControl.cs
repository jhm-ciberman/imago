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
                    if (this.Root != null)
                    {
                        this._content.OnRemovedFromVisualTree(this.Root);
                    }
                }

                this._content = value;

                if (this._content != null)
                {
                    this._content.Parent = this;
                    if (this.Root != null)
                    {
                        this._content.OnAddedToVisualTree(this.Root);
                    }
                    this._visualChildren = new[] { this._content };
                }
                else
                {
                    this._visualChildren = Array.Empty<Control>();
                }

                this.InvalidateMeasure();
            }
        }
    }

    protected override Vector2 MeasureCore(Vector2 availableSize)
    {
        if (this.Content != null)
        {
            this.Content.Measure(availableSize);
            return this.Content.DesiredSize;
        }
        else
        {
            return Vector2.Zero;
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
        base.DrawCore(spriteBatcher);

        if (this.Content != null)
        {
            this.Content.Draw(spriteBatcher);
        }
    }

    public override void Update(float deltaTime)
    {
        this.Content?.Update(deltaTime);

        base.Update(deltaTime);
    }
}