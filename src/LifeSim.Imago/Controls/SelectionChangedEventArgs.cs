using System;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Provides data for selection changed events.
/// </summary>
public class SelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// Gets the previously selected item, or null if there was no selection.
    /// </summary>
    public ToggleButton? OldItem { get; }

    /// <summary>
    /// Gets the newly selected item, or null if the selection was cleared.
    /// </summary>
    public ToggleButton? NewItem { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionChangedEventArgs"/> class.
    /// </summary>
    /// <param name="oldItem">The previously selected item.</param>
    /// <param name="newItem">The newly selected item.</param>
    public SelectionChangedEventArgs(ToggleButton? oldItem, ToggleButton? newItem)
    {
        this.OldItem = oldItem;
        this.NewItem = newItem;
    }
}
