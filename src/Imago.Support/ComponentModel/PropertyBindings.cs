using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Imago.Support.ComponentModel;

/// <summary>
/// Groups property change subscriptions on a single <see cref="INotifyPropertyChanged"/>
/// source. Each registered callback runs once immediately and again whenever the named
/// property changes, so initial values and reactive updates live in one place. Disposing
/// the binder unsubscribes from the source.
/// </summary>
/// <typeparam name="TSource">The notifying source type.</typeparam>
/// <example>
/// <code>
/// this._bindings = new PropertyBindings&lt;MyViewModel&gt;(this._vm)
///     .Watch(nameof(MyViewModel.Title),  vm =&gt; this._title.Text = vm.Title)
///     .Watch(nameof(MyViewModel.Health), vm =&gt; this._healthBar.Value = vm.Health);
/// </code>
/// </example>
public sealed class PropertyBindings<TSource> : IDisposable
    where TSource : INotifyPropertyChanged
{
    private readonly TSource _source;
    private readonly Dictionary<string, Action> _handlers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyBindings{TSource}"/> class
    /// and subscribes to <paramref name="source"/>.
    /// </summary>
    /// <param name="source">The notifying source whose property changes drive registered callbacks.</param>
    public PropertyBindings(TSource source)
    {
        this._source = source;
        this._source.PropertyChanged += this.Source_PropertyChanged;
    }

    /// <summary>
    /// Registers a callback that runs immediately and again whenever the named property
    /// changes on the source. Multiple callbacks for the same property are all invoked
    /// in registration order.
    /// </summary>
    /// <param name="propertyName">The name of the source property to watch.</param>
    /// <param name="apply">The callback to run on registration and on each change.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    public PropertyBindings<TSource> Watch(string propertyName, Action apply)
    {
        apply();
        if (this._handlers.TryGetValue(propertyName, out var existing))
        {
            this._handlers[propertyName] = existing + apply;
        }
        else
        {
            this._handlers[propertyName] = apply;
        }
        return this;
    }

    /// <summary>
    /// Registers a callback that runs immediately and again whenever the named property
    /// changes on the source. The current source is passed to the callback so it can be
    /// used without a closure over an outer field.
    /// </summary>
    /// <param name="propertyName">The name of the source property to watch.</param>
    /// <param name="apply">The callback to run on registration and on each change.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    public PropertyBindings<TSource> Watch(string propertyName, Action<TSource> apply)
    {
        return this.Watch(propertyName, () => apply(this._source));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._source.PropertyChanged -= this.Source_PropertyChanged;
        this._handlers.Clear();
    }

    private void Source_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == null)
        {
            foreach (var handler in this._handlers.Values)
            {
                handler();
            }
            return;
        }

        if (this._handlers.TryGetValue(e.PropertyName, out var apply))
        {
            apply();
        }
    }
}
