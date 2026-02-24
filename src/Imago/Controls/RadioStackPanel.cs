using System;

namespace Imago.Controls;

/// <summary>
/// A <see cref="StackPanel"/> that automatically manages mutual exclusivity of <see cref="ToggleButton"/> children
/// through an internal <see cref="RadioGroup"/>.
/// </summary>
public class RadioStackPanel : StackPanel
{
    private readonly RadioGroup _radioGroup = new RadioGroup();

    /// <summary>
    /// Occurs when the selected item changes.
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => this._radioGroup.SelectionChanged += value;
        remove => this._radioGroup.SelectionChanged -= value;
    }

    /// <summary>
    /// Gets or sets the currently selected (checked) toggle button, or null if no item is selected.
    /// </summary>
    public ToggleButton? SelectedItem
    {
        get => this._radioGroup.SelectedItem;
        set => this._radioGroup.SelectedItem = value;
    }

    /// <summary>
    /// Gets or sets the index of the currently selected item. A value of -1 indicates no selection.
    /// </summary>
    public int SelectedIndex
    {
        get => this._radioGroup.SelectedIndex;
        set => this._radioGroup.SelectedIndex = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the selected item can be deselected by clicking it again.
    /// When false, clicking the active toggle button has no effect.
    /// </summary>
    public bool AllowDeselection
    {
        get => this._radioGroup.AllowDeselection;
        set => this._radioGroup.AllowDeselection = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioStackPanel"/> class.
    /// </summary>
    public RadioStackPanel()
    {
        //
    }

    /// <inheritdoc/>
    protected override void AddVisualChild(Visual child)
    {
        base.AddVisualChild(child);

        if (child is ToggleButton toggleButton)
        {
            this._radioGroup.Add(toggleButton);
        }
    }

    /// <inheritdoc/>
    protected override void RemoveVisualChild(Visual child)
    {
        if (child is ToggleButton toggleButton)
        {
            this._radioGroup.Remove(toggleButton);
        }

        base.RemoveVisualChild(child);
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this._radioGroup.Dispose();
        }

        base.Dispose(disposing);
    }
}
