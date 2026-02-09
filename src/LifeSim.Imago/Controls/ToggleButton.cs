using System;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a button that maintains a checked/unchecked state with distinct visual appearances for each.
/// </summary>
public class ToggleButton : Button
{
    /// <summary>
    /// Occurs when the <see cref="IsChecked"/> property changes.
    /// </summary>
    public event EventHandler? CheckedChanged;

    private bool _isChecked;
    private ButtonAppearance _checkedAppearance = ButtonAppearance.Default;

    /// <summary>
    /// Gets or sets a value indicating whether this toggle button is in the checked (active) state.
    /// </summary>
    public bool IsChecked
    {
        get => this._isChecked;
        set
        {
            if (this._isChecked == value) return;

            this._isChecked = value;
            this.OnPropertyChanged(nameof(this.IsChecked));
            this.CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets or sets the appearance used when <see cref="IsChecked"/> is true.
    /// </summary>
    public ButtonAppearance CheckedAppearance
    {
        get => this._checkedAppearance;
        set
        {
            if (this._checkedAppearance == value) return;

            this._checkedAppearance = value;
            if (this.IsChecked)
            {
                this._checkedAppearance.Apply(this);
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleButton"/> class.
    /// </summary>
    public ToggleButton()
    {
        this.Click += this.ToggleButton_Click;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleButton"/> class with the specified content.
    /// </summary>
    /// <param name="content">The content of the button.</param>
    public ToggleButton(Control? content) : this()
    {
        this.Content = content;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleButton"/> class with the specified text.
    /// </summary>
    /// <param name="text">The text to display on the button.</param>
    public ToggleButton(string text) : this()
    {
        this.Content = new TextBlock(text);
    }

    /// <inheritdoc/>
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (this.Layer == null) return;

        var appearance = this.IsChecked ? this.CheckedAppearance : this.Appearance;
        appearance.Apply(this);
    }

    private void ToggleButton_Click(object? sender, EventArgs e)
    {
        this.IsChecked = !this.IsChecked;
    }
}
