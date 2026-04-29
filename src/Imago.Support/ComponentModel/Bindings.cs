using System;
using System.Collections;
using System.Collections.Generic;

namespace Imago.Support.ComponentModel;

/// <summary>
/// Composite collection of disposable bindings. Owns its items and disposes them all
/// when the group itself is disposed. Any <see cref="IDisposable"/> can be added,
/// which lets callers mix property bindings, input bindings, and any future
/// binding types in a single declaration.
/// </summary>
/// <example>
/// <code>
/// return new Bindings
/// {
///     new PropertyBindings&lt;MyViewModel&gt;(this._vm)
///         .Watch(nameof(MyViewModel.Title),  vm =&gt; this._title.Text = vm.Title)
///         .Watch(nameof(MyViewModel.Health), vm =&gt; this._healthBar.Value = vm.Health),
///
///     new InputBindings()
///         .Pressed(GameAction.GoUpStorey, this._vm.GoUpStoreyCommand),
/// };
/// </code>
/// </example>
public sealed class Bindings : IEnumerable, IDisposable
{
    private readonly List<IDisposable> _items = new();

    /// <summary>
    /// Adds a disposable binding to this group.
    /// </summary>
    /// <param name="item">The binding to add. Disposed when the group is disposed.</param>
    /// <returns>This instance, for fluent chaining.</returns>
    public Bindings Add(IDisposable item)
    {
        this._items.Add(item);
        return this;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var item in this._items)
        {
            item.Dispose();
        }
        this._items.Clear();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        yield break;
    }
}
