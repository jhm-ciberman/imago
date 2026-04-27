using System;

namespace Imago.Controls;

/// <summary>
/// Represents a data template that turns an item into a control.
/// </summary>
/// <remarks>
/// Implementations are responsible for declaring which items they apply to via
/// <see cref="Match"/>. A composite template such as <see cref="DataTemplates"/>
/// uses <see cref="Match"/> to dispatch to the right child template at runtime,
/// so callers selecting a template polymorphically should always check
/// <see cref="Match"/> before calling <see cref="CreateItem"/>.
/// </remarks>
public interface IDataTemplate
{
    /// <summary>
    /// Determines whether this template can produce a control for the specified item.
    /// </summary>
    /// <param name="item">The item to test.</param>
    /// <returns><see langword="true"/> if <see cref="CreateItem"/> can be invoked with this item.</returns>
    public bool Match(object item);

    /// <summary>
    /// Creates a control for the specified item.
    /// </summary>
    /// <param name="item">The item to create a control for.</param>
    /// <returns>The created control.</returns>
    public Control CreateItem(object item);
}

/// <summary>
/// A typed data template that produces a control from items of type <typeparamref name="T"/>.
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
    bool IDataTemplate.Match(object item)
    {
        return item is T;
    }

    /// <inheritdoc/>
    Control IDataTemplate.CreateItem(object item)
    {
        return this._factory.Invoke((T)item);
    }
}
