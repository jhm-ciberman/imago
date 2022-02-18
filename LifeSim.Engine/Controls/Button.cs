using System;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class Button : Control
{
    public Control? Content { get; set; }

    protected override void ArrangeCore(Rectangle finalRect)
    {
        if (this.Content != null)
        {
            this.Content.Arrange(finalRect);
        }
    }

    protected override void MeasureCore(Vector2 availableSize)
    {
        if (this.Content != null)
        {
            this.Content.Measure(availableSize);
            this.DesiredSize = this.Content.DesiredSize;
        }
        else
        {
            this.DesiredSize = Vector2.Zero;
        }
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        if (this.Content != null)
        {
            this.Content.Draw(spriteBatcher);
        }
    }

    public Button()
    {
        //
    }

    public Button(Control? content)
    {
        this.Content = content;
    }

    public Button(string text)
    {
        this.Content = new TextBlock(text);
    }
}