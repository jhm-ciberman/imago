using System;
using System.Collections.Generic;

namespace Imago.Controls;

/// <summary>
/// A non-visual coordinator that enforces mutual exclusivity among a set of <see cref="ToggleButton"/> instances.
/// Buttons managed by this group can live in any layout container.
/// </summary>
public class RadioGroup : IDisposable
{
    /// <summary>
    /// Occurs when the selected item changes.
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

    private readonly List<ToggleButton> _items = new();
    private ToggleButton? _selectedItem;
    private int _selectedIndex = -1;
    private bool _allowDeselection;
    private bool _isUpdatingSelection;

    /// <summary>
    /// Gets the collection of <see cref="ToggleButton"/> instances managed by this group.
    /// </summary>
    public IReadOnlyList<ToggleButton> Items => this._items;

    /// <summary>
    /// Gets or sets the currently selected (checked) toggle button, or null if no item is selected.
    /// </summary>
    public ToggleButton? SelectedItem
    {
        get => this._selectedItem;
        set
        {
            if (this._selectedItem == value) return;

            if (value != null)
            {
                value.IsChecked = true;
            }
            else
            {
                this.SetSelectedItem(null);
            }
        }
    }

    /// <summary>
    /// Gets or sets the index of the currently selected item. A value of -1 indicates no selection.
    /// </summary>
    public int SelectedIndex
    {
        get => this._selectedIndex;
        set
        {
            if (value < 0 || value >= this._items.Count)
            {
                this.SelectedItem = null;
            }
            else
            {
                this.SelectedItem = this._items[value];
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the selected item can be deselected by clicking it again.
    /// When false, clicking the active toggle button has no effect.
    /// </summary>
    public bool AllowDeselection
    {
        get => this._allowDeselection;
        set => this._allowDeselection = value;
    }

    /// <summary>
    /// Adds a <see cref="ToggleButton"/> to this group.
    /// </summary>
    /// <param name="item">The toggle button to add.</param>
    public void Add(ToggleButton item)
    {
        this._items.Add(item);
        item.CheckedChanged += this.ToggleButton_CheckedChanged;

        if (item.IsChecked)
        {
            this.SetSelectedItem(item);
        }
    }

    /// <summary>
    /// Removes a <see cref="ToggleButton"/> from this group.
    /// </summary>
    /// <param name="item">The toggle button to remove.</param>
    public void Remove(ToggleButton item)
    {
        item.CheckedChanged -= this.ToggleButton_CheckedChanged;
        this._items.Remove(item);

        if (this._selectedItem == item)
        {
            this.SetSelectedItem(null);
        }
    }

    private void ToggleButton_CheckedChanged(object? sender, EventArgs e)
    {
        if (this._isUpdatingSelection) return;
        if (sender is not ToggleButton toggleButton) return;

        if (toggleButton.IsChecked)
        {
            this.SetSelectedItem(toggleButton);
        }
        else if (this._selectedItem == toggleButton)
        {
            if (!this._allowDeselection)
            {
                this._isUpdatingSelection = true;
                try
                {
                    toggleButton.IsChecked = true;
                }
                finally
                {
                    this._isUpdatingSelection = false;
                }
            }
            else
            {
                this.SetSelectedItem(null);
            }
        }
    }

    private void SetSelectedItem(ToggleButton? newItem)
    {
        if (this._selectedItem == newItem) return;

        this._isUpdatingSelection = true;
        try
        {
            var oldItem = this._selectedItem;

            if (oldItem != null)
            {
                oldItem.IsChecked = false;
            }

            this._selectedItem = newItem;
            this._selectedIndex = newItem != null ? this._items.IndexOf(newItem) : -1;

            this.SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(oldItem, newItem));
        }
        finally
        {
            this._isUpdatingSelection = false;
        }
    }

    /// <summary>
    /// Removes all toggle buttons from this group and unhooks all events.
    /// </summary>
    public void Dispose()
    {
        foreach (var item in this._items)
        {
            item.CheckedChanged -= this.ToggleButton_CheckedChanged;
        }

        this._items.Clear();
        this._selectedItem = null;
        this._selectedIndex = -1;
    }
}
