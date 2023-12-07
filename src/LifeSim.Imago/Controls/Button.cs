using System;
using System.Numerics;
using System.Windows.Input;
using LifeSim.Imago.Controls.Drawing;
using LifeSim.Imago.Graphics;
using LifeSim.Imago.Graphics.Rendering;
using LifeSim.Support.Drawing;
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

    private ButtonVisualState _visualState = ButtonVisualState.Default;

    public ButtonVisualState VisualState
    {
        get => this._visualState;
        set
        {
            if (this._visualState == value) return;

            this._visualState = value;
            this._visualState.ApplyVisualState(this);
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

        this.VisualState.ApplyVisualState(this);

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

public class ButtonVisualState
{
    private static readonly ButtonVisualState _default = new ButtonVisualState();

    public static ButtonVisualState Default => _default;

    public IBackground Idle { get; set; } = new ColorBackground(Color.White);

    public IBackground Hover { get; set; } = new ColorBackground(Color.White, 0.9f);

    public IBackground Pressed { get; set; } = new ColorBackground(Color.White, 0.8f);

    public IBackground Disabled { get; set; } = new ColorBackground(Color.White, 0.5f);


    public ButtonVisualState()
    {
        //
    }

    public void ApplyVisualState(Button button)
    {
        if (button.IsMouseOver)
        {
            button.Background = this.Hover;
        }
        else if (button.IsPressed)
        {
            button.Background = this.Pressed;
        }
        else if (!button.IsEnabled)
        {
            button.Background = this.Disabled;
        }
        else
        {
            button.Background = this.Idle;
        }
    }

    public static ButtonVisualState FromColor(Color color)
    {
        return new ButtonVisualState()
        {
            Idle = new ColorBackground(color),
            Hover = new ColorBackground(color, 0.9f),
            Pressed = new ColorBackground(color, 0.8f),
            Disabled = new ColorBackground(color, 0.5f),
        };
    }

    public static ButtonVisualState FromSprite(Sprite sprite)
    {
        return new ButtonVisualState()
        {
            Idle = new SpriteBackground(sprite, 0),
            Hover = new SpriteBackground(sprite, 1),
            Pressed = new SpriteBackground(sprite, 2),
            Disabled = new SpriteBackground(sprite, 3),
        };
    }

    public static ButtonVisualState FromColors(Color defaultColor, Color hoverColor, Color pressedColor, Color disabledColor)
    {
        return new ButtonVisualState()
        {
            Idle = new ColorBackground(defaultColor),
            Hover = new ColorBackground(hoverColor),
            Pressed = new ColorBackground(pressedColor),
            Disabled = new ColorBackground(disabledColor),
        };
    }
}
