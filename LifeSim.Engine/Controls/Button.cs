using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.Controls;

public class Button : ContentControl
{
    public Action<Button>? Click { get; set; }

    public Action<Button>? MouseEnter { get; set; }

    public Action<Button>? MouseLeave { get; set; }

    private bool _isMouseOver = false;

    public bool IsMouseOver
    {
        get => this._isMouseOver;
        protected set
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
        protected set
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

    public Texture? NormalTexture { get; set; }

    public Texture? HoverTexture { get; set; }

    public Texture? PressedTexture { get; set; }

    public Shader? Shader { get; set; }




    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

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
            spriteBatcher.DrawTexture(this.Shader, texture, this.Position, this.ActualSize);
        }
    }

    public override void Update(float deltaTime)
    {
        if (this.Root == null) return;
        if (this.Content != null)
        {
            this.Content.Update(deltaTime);
        }
        Vector2 mousePosition = Input.MousePosition / this.Root.Zoom;
        Rect bounds = new Rect(this.Position, this.ActualSize);
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

        base.Update(deltaTime);
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