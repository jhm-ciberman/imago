using System;
using System.Windows.Input;
using LifeSim.Imago.Input;
using LifeSim.Imago.Rendering.Sprites;
using Veldrid;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a clickable button control.
/// </summary>
public class Button : ContentControl
{
    /// <summary>
    /// Occurs when the button is clicked.
    /// </summary>
    public event EventHandler? Click;

    private bool _isPressed = false;

    /// <summary>
    /// Gets a value indicating whether the button is currently in the pressed state.
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
    /// Gets or sets a value indicating whether the button is enabled and can be interacted with.
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    private ICommand? _command = null;
    private object? _commandParameter = null;

    /// <summary>
    /// Gets or sets the command to execute when the button is clicked.
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

    /// <summary>
    /// Handles changes in the <see cref="ICommand.CanExecute"/> status, updating the button's <see cref="IsEnabled"/> state accordingly.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnCommandCanExecuteChanged(object? sender, EventArgs e)
    {
        this.IsEnabled = this.Command?.CanExecute(this.CommandParameter) ?? true;
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the <see cref="Command"/> property.
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

    /// <summary>
    /// Gets or sets the appearance of the button.
    /// </summary>
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

    /// <inheritdoc/>
    protected override void DrawCore(DrawingContext ctx)
    {
        base.DrawCore(ctx);
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public override void HandleMousePressed(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            this.IsPressed = true;
            e.Handled = true; // Prevent further propagation of the event
        }

        base.HandleMousePressed(e);
    }

    /// <inheritdoc/>
    public override void HandleMouseReleased(MouseButtonEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            this.IsPressed = false;
            e.Handled = true; // Prevent further propagation of the event
        }

        base.HandleMouseReleased(e);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class.
    /// </summary>
    public Button()
    {
        //
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class with the specified content.
    /// </summary>
    /// <param name="content">The content of the button.</param>
    public Button(Control? content)
    {
        this.Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Button"/> class with the specified text.
    /// </summary>
    /// <param name="text">The text to display on the button.</param>
    public Button(string text)
    {
        this.Content = new TextBlock(text);
    }
}
