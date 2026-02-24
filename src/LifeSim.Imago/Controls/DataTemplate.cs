using System;

namespace LifeSim.Imago.Controls;

/// <summary>
/// Represents a data template that defines the appearance and behavior of an item in a user interface.
/// </summary>
public interface IDataTemplate
{
    /// <summary>
    /// Creates a control for the specified item.
    /// </summary>
    /// <param name="item">The item to create a control for.</param>
    /// <returns>The created control.</returns>
    public Control CreateItem(object item);
}

/// <summary>
/// Represents a generic data template that defines the appearance and behavior of an item in a user interface.
/// </summary>
/// <typeparam name="T">The type of the item.</typeparam>
[FactoryTemplate]
public class DataTemplate<T> : IDataTemplate
{
    private readonly Func<T, Control> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataTemplate{T}"/> class with the specified factory function.
    /// </summary>
    /// <param name="factory">The factory function that creates a control for the specified item.</param>
    public DataTemplate(Func<T, Control> factory)
    {
        this._factory = factory;
    }

    /// <inheritdoc/>
    Control IDataTemplate.CreateItem(object item)
    {
        return this._factory.Invoke((T)item);
    }
}
