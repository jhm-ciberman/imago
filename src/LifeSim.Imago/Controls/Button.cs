using System;
using System.Numerics;
using System.Windows.Input;
using LifeSim.Imago.Rendering.Sprites;
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

    private ButtonAppearance _appearance = ButtonAppearance.Default;

    public ButtonAppearance Appearance
    {
        get => this._appearance;
        set
        {
            if (this._appearance == value) return;

            this._appearance = value;
            this._appearance.Apply(this);
        }
    }

    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);
    }

    public override void Update(float deltaTime)
    {
        if (this.Stage == null) return;

        this.Content?.Update(deltaTime);

        base.Update(deltaTime);

        this.Appearance.Apply(this);

        if (this.IsPressed && !this.IsMouseOver && this.Stage.Input.WasMouseButtonReleasedThisFrame(MouseButton.Left))
        {
            this.IsPressed = false;
        }
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
