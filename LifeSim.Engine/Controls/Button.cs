using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class Button : Control
{
    private Control? _content;
    public Control? Content
    {
        get => this._content;
        set
        {
            this._content = value;
            this._visualChildren = value == null ? Array.Empty<Control>() : new[] { value };
        }
    }

    public Action<Button>? Click { get; set; }

    public Action<Button>? MouseEnter { get; set; }

    public Action<Button>? MouseLeave { get; set; }

    private bool _isMouseOver = false;

    public bool IsMouseOver
    {
        get => this._isMouseOver;
        set
        {
            if (this._isMouseOver != value)
            {
                this._isMouseOver = value;
                if (this._isMouseOver)
                {
                    this.MouseEnter?.Invoke(this);
                }
                else
                {
                    this.MouseLeave?.Invoke(this);
                }
            }
        }
    }

    private bool _isPressed = false;

    public bool IsPressed
    {
        get => this._isPressed;
        set
        {
            if (this._isPressed != value)
            {
                this._isPressed = value;
                if (this._isPressed)
                {
                    this.Click?.Invoke(this);
                }
            }
        }
    }

    public Rendering.Texture? NormalTexture { get; set; }

    public Rendering.Texture? HoverTexture { get; set; }

    public Rendering.Texture? PressedTexture { get; set; }

    public Shader? Shader { get; set; }

    private IEnumerable<Control> _visualChildren = Array.Empty<Control>();
    public override IEnumerable<Control> VisualChildren => this._visualChildren;

    protected override void ArrangeCore(Rectangle finalRect)
    {
        if (this.Content != null)
        {
            this.Content.Arrange(finalRect);
        }

        this.Position = finalRect.Position;
        this.ActualSize = finalRect.Size;
    }

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

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        var texture = this.NormalTexture;
        if (this.IsPressed)
        {
            texture = this.PressedTexture;
        }
        else if (this.IsMouseOver)
        {
            texture = this.HoverTexture;
        }

        if (texture != null)
        {
            spriteBatcher.Draw(this.Shader, texture, this.Position, this.ActualSize);
        }

        if (this.Content != null)
        {
            this.Content.Draw(spriteBatcher);
        }
    }

    public override void Update(float deltaTime)
    {
        if (this.Content != null)
        {
            this.Content.Update(deltaTime);
        }

        Vector2 mousePosition = Input.MousePosition;
        Rectangle bounds = new Rectangle(this.Position, this.ActualSize);
        if (bounds.Contains(mousePosition))
        {
            this.IsMouseOver = true;

            if (Input.GetMouseButtonDown(Veldrid.MouseButton.Left))
            {
                this.IsPressed = true;
            }
        }
        else
        {
            this.IsMouseOver = false;
        }

        if (this.IsPressed && Input.GetMouseButtonUp(Veldrid.MouseButton.Left))
        {
            this.IsPressed = false;
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