using System;
using System.Numerics;
using LifeSim.Engine.Rendering;
using LifeSim.Engine.Resources;

namespace LifeSim.Engine.Controls;

public class Button : ContentControl
{
    /// <summary>
    /// Event that is raised when the button is clicked.
    /// </summary>
    public Action<Button>? Click { get; set; }

    /// <summary>
    /// Event that is raised when the mouse enters the button.
    /// </summary>
    public Action<Button>? MouseEnter { get; set; }

    /// <summary>
    /// Event that is raised when the mouse leaves the button.
    /// </summary>
    public Action<Button>? MouseLeave { get; set; }

    private bool _isMouseOver = false;

    /// <summary>
    /// Gets whether the mouse is currently over the button.
    /// </summary>
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

    /// <summary>
    /// Gets whether the button is currently pressed.
    /// </summary>
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

    /// <summary>
    /// Gets or sets whether the button is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the button default texture.
    /// </summary>
    public PackedTexture? NormalTexture { get; set; }

    /// <summary>
    /// Gets or sets the button mouse over texture.
    /// </summary>
    public PackedTexture? HoverTexture { get; set; }

    /// <summary>
    /// Gets or sets the button pressed texture.
    /// </summary>
    public PackedTexture? PressedTexture { get; set; }

    /// <summary>
    /// Gets or sets the button disabled texture.
    /// </summary>
    public PackedTexture? DisabledTexture { get; set; }


    public Shader? Shader { get; set; }


    protected PackedTexture? GetCurrentTexture()
    {
        if (!this.IsEnabled)
        {
            return this.DisabledTexture ?? this.NormalTexture;
        }
        else if (this.IsPressed)
        {
            return this.PressedTexture;
        }
        else if (this.IsMouseOver)
        {
            return this.HoverTexture ?? this.NormalTexture;
        }
        else
        {
            return this.NormalTexture;
        }
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);

        var texture = this.GetCurrentTexture();
        if (texture != null)
        {
            spriteBatcher.DrawTexture(this.Shader, texture.Texture, this.Position, this.ActualSize, texture.TopLeft, texture.BottomRight, Color.White);
        }
    }

    public override void Update(float deltaTime)
    {
        if (this.Root == null) return;
        if (this.Content != null)
        {
            this.Content.Update(deltaTime);
        }
        Vector2 mousePosition = Input.Instance.MousePosition / this.Root.Zoom;
        Rect bounds = new Rect(this.Position, this.ActualSize);
        if (bounds.Contains(mousePosition))
        {
            this.IsMouseOver = true;

            if (Input.Instance.GetMouseButtonDown(Veldrid.MouseButton.Left))
            {
                this.IsPressed = true;
            }
        }
        else
        {
            this.IsMouseOver = false;
        }

        if (this.IsPressed && Input.Instance.GetMouseButtonUp(Veldrid.MouseButton.Left))
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