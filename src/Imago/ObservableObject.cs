using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Imago;

/// <summary>
/// Base class for objects that notify listeners when one of their properties changes.
/// </summary>
public abstract class ObservableObject : INotifyPropertyChanged
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableObject"/> class.
    /// </summary>
    internal ObservableObject()
    {
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="e">The event arguments describing which property changed.</param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        this.PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the given property name.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed. Supplied automatically when omitted.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Assigns <paramref name="value"/> to <paramref name="field"/> and raises <see cref="PropertyChanged"/>
    /// if the new value differs from the current one.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="field">A reference to the backing field.</param>
    /// <param name="value">The new value to assign.</param>
    /// <param name="propertyName">The name of the property being set. Supplied automatically when omitted.</param>
    /// <returns><see langword="true"/> if the value changed and the event was raised; otherwise <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        this.OnPropertyChanged(propertyName);
        return true;
    }
}
