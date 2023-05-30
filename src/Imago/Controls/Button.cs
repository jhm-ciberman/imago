using System;
using System.Numerics;
using Imago.Input;
using Imago.Rendering;
using Support;
using Support.ComponentModel;

namespace Imago.Controls;

public class Button : ContentControl
{
    /// <summary>
    /// Event that is raised when the button is clicked.
    /// </summary>
    public event EventHandler? Click;

    /// <summary>
    /// Event that is raised when the mouse enters the button.
    /// </summary>
    public event EventHandler? MouseEnter;

    /// <summary>
    /// Event that is raised when the mouse leaves the button.
    /// </summary>
    public event EventHandler? MouseLeave;

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
                    this.MouseEnter?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    this.MouseLeave?.Invoke(this, EventArgs.Empty);
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
                if (!this._isPressed && this.IsMouseOver)
                {
                    this.Click?.Invoke(this, EventArgs.Empty);

                    if (this.Command != null)
                    {
                        if (this.Command.CanExecute(this.CommandParameter))
                        {
                            this.Command.Execute(this.CommandParameter);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets whether the button is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Command that is executed when the button is clicked.
    /// </summary>
    public ICommand? Command { get; set; } = null;

    /// <summary>
    /// Parameter that is passed to the command.
    /// </summary>
    public object? CommandParameter { get; set; } = null;

    protected void UpdateBackgroundBrush()
    {
        var background = this.Background;
        if (background == null) return;

        if (background is SpriteBrush spriteBrush)
        {
            if (spriteBrush.Sprite == null) return;
            if (this.IsEnabled)
            {
                if (this.IsPressed)
                {
                    spriteBrush.FrameIndex = 2;
                }
                else if (this.IsMouseOver)
                {
                    spriteBrush.FrameIndex = 1;
                }
                else
                {
                    spriteBrush.FrameIndex = 0;
                }
            }
            else
            {
                spriteBrush.FrameIndex = 3;
            }
        }
        else if (background is SolidColorBrush solidColor)
        {
            // Change opacity
            if (this.IsEnabled)
            {
                if (this.IsPressed)
                {
                    solidColor.Opacity = 0.8f;
                }
                else if (this.IsMouseOver)
                {
                    solidColor.Opacity = 0.9f;
                }
                else
                {
                    solidColor.Opacity = 1.0f;
                }
            }
            else
            {
                solidColor.Opacity = 0.5f;
            }
        }
    }

    protected override void DrawCore(SpriteBatcher spriteBatcher)
    {
        base.DrawCore(spriteBatcher);
    }

    public override void Update(float deltaTime)
    {
        if (this.Stage == null) return;

        this.Content?.Update(deltaTime);

        Vector2 mousePosition = InputManager.Current.MousePosition / this.Stage.Zoom;
        Rect bounds = new Rect(this.Position, this.ActualSize);

        if (bounds.Contains(mousePosition))
        {
            this.IsMouseOver = true;

            if (InputManager.Current.GetMouseButtonDown(Veldrid.MouseButton.Left))
            {
                this.IsPressed = true;
            }
        }
        else
        {
            this.IsMouseOver = false;
            this.IsPressed = false;
        }

        if (this.IsPressed && InputManager.Current.GetMouseButtonUp(Veldrid.MouseButton.Left))
        {
            this.IsPressed = false;
        }

        this.UpdateBackgroundBrush();

        base.Update(deltaTime);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    public Button()
    {
        //
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    /// <param name="content">The content of the button.</param>
    public Button(Control? content)
    {
        this.Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    /// <param name="text">The text of the button.</param>
    public Button(string text)
    {
        this.Content = new TextBlock(text);
    }
}
