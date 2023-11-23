using System;
using System.Numerics;
using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Graphics.Rendering;
using LifeSim.Support.ComponentModel;
using Veldrid;

namespace LifeSim.Imago.Controls;

public class Button : ContentControl
{
    /// <summary>
    /// Occurs when the button is clicked.
    /// </summary>
    public event EventHandler? Click;

    private bool _isPressed = false;

    /// <summary>
    /// Gets whether the button is currently pressed.
    /// </summary>
    public bool IsPressed
    {
        get => this._isPressed;
        protected set
        {
            if (this._isPressed == value) return;

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

    /// <summary>
    /// Gets or sets whether the button is enabled.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    private ICommand? _command = null;
    private object? _commandParameter = null;

    /// <summary>
    /// Command that is executed when the button is clicked.
    /// </summary>
    public ICommand? Command
    {
        get => this._command;
        set
        {
            if (this._command == value) return;

            if (this._command != null)
            {
                this._command.CanExecuteChanged -= this.OnCommandCanExecuteChanged;
            }

            this._command = value;
            this.IsEnabled = this.Command?.CanExecute(this.CommandParameter) ?? true;

            if (this._command != null)
            {
                this._command.CanExecuteChanged += this.OnCommandCanExecuteChanged;
            }
        }
    }

    protected virtual void OnCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        this.IsEnabled = this.Command?.CanExecute(this.CommandParameter) ?? true;
    }

    /// <summary>
    /// Parameter that is passed to the command.
    /// </summary>
    public object? CommandParameter
    {
        get => this._commandParameter;
        set
        {
            if (this._commandParameter == value) return;

            this._commandParameter = value;
            this.IsEnabled = this.Command?.CanExecute(this.CommandParameter) ?? true;
        }
    }

    /// <summary>
    /// Gets or sets the background brush of the button.
    /// </summary>
    protected void UpdateBackgroundBrush()
    {
        var background = this.Background;
        if (background == null) return;

        if (background is SpriteBackground spriteBrush)
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
        else if (background is ColorBackground solidColor)
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

        this.UpdateBackgroundBrush();

        base.Update(deltaTime);
    }

    protected override void OnMouseDown(MouseButton button, Vector2 position)
    {
        base.OnMouseDown(button, position);

        if (button == MouseButton.Left)
        {
            this.IsPressed = true;
        }
    }

    protected override void OnMouseUp(MouseButton button, Vector2 position)
    {
        base.OnMouseUp(button, position);

        if (button == MouseButton.Left)
        {
            this.IsPressed = false;
        }
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
