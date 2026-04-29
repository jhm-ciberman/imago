using System;

namespace Imago.Support.ComponentModel;

/// <summary>
/// Subscribes to an <see cref="EventHandler"/>-style event for the lifetime of this
/// instance. The caller supplies add / remove lambdas that wrap the event's
/// <c>+=</c> / <c>-=</c> operators, and disposing the binding unsubscribes the handler.
/// </summary>
/// <example>
/// <code>
/// new EventBinding(
///     h =&gt; source.Changed += h,
///     h =&gt; source.Changed -= h,
///     () =&gt; this.OnSourceChanged());
/// </code>
/// </example>
public sealed class EventBinding : IDisposable
{
    private readonly Action _unsubscribe;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBinding"/> class and
    /// subscribes <paramref name="handler"/> via the given add/remove lambdas.
    /// </summary>
    /// <param name="add">Lambda that adds a handler to the event (<c>h =&gt; source.X += h</c>).</param>
    /// <param name="remove">Lambda that removes a handler from the event (<c>h =&gt; source.X -= h</c>).</param>
    /// <param name="handler">The handler to invoke when the event fires.</param>
    public EventBinding(
        Action<EventHandler> add,
        Action<EventHandler> remove,
        EventHandler handler
    )
    {
        add(handler);
        this._unsubscribe = () => remove(handler);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBinding"/> class with a
    /// wrapper handler that ignores the event arguments and calls <paramref name="handler"/>.
    /// </summary>
    /// <param name="add">Lambda that adds a handler to the event.</param>
    /// <param name="remove">Lambda that removes a handler from the event.</param>
    /// <param name="handler">The parameterless callback to invoke when the event fires.</param>
    public EventBinding(
        Action<EventHandler> add,
        Action<EventHandler> remove,
        Action handler
    )
        : this(add, remove, (_, _) => handler())
    {
    }

    /// <inheritdoc />
    public void Dispose() => this._unsubscribe();
}

/// <summary>
/// Subscribes to an <see cref="EventHandler{TArgs}"/>-style event for the lifetime
/// of this instance. The caller supplies add / remove lambdas that wrap the event's
/// <c>+=</c> / <c>-=</c> operators, and disposing the binding unsubscribes the handler.
/// </summary>
/// <typeparam name="TArgs">The event argument type.</typeparam>
/// <example>
/// <code>
/// new EventBinding&lt;TerrainChangedEventArgs&gt;(
///     h =&gt; world.TerrainChanged += h,
///     h =&gt; world.TerrainChanged -= h,
///     this.OnTerrainChanged);
/// </code>
/// </example>
public sealed class EventBinding<TArgs> : IDisposable
{
    private readonly Action _unsubscribe;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBinding{TArgs}"/> class and
    /// subscribes <paramref name="handler"/> via the given add/remove lambdas.
    /// </summary>
    /// <param name="add">Lambda that adds a handler to the event.</param>
    /// <param name="remove">Lambda that removes a handler from the event.</param>
    /// <param name="handler">The handler to invoke when the event fires.</param>
    public EventBinding(
        Action<EventHandler<TArgs>> add,
        Action<EventHandler<TArgs>> remove,
        EventHandler<TArgs> handler
    )
    {
        add(handler);
        this._unsubscribe = () => remove(handler);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBinding{TArgs}"/> class with
    /// a wrapper handler that ignores the event arguments and calls <paramref name="handler"/>.
    /// </summary>
    /// <param name="add">Lambda that adds a handler to the event.</param>
    /// <param name="remove">Lambda that removes a handler from the event.</param>
    /// <param name="handler">The parameterless callback to invoke when the event fires.</param>
    public EventBinding(
        Action<EventHandler<TArgs>> add,
        Action<EventHandler<TArgs>> remove,
        Action handler
    )
        : this(add, remove, (_, _) => handler())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventBinding{TArgs}"/> class with
    /// a wrapper handler that ignores the sender and forwards the event arguments to
    /// <paramref name="handler"/>.
    /// </summary>
    /// <param name="add">Lambda that adds a handler to the event.</param>
    /// <param name="remove">Lambda that removes a handler from the event.</param>
    /// <param name="handler">The callback to invoke with the event arguments.</param>
    public EventBinding(
        Action<EventHandler<TArgs>> add,
        Action<EventHandler<TArgs>> remove,
        Action<TArgs> handler
    )
        : this(add, remove, (_, e) => handler(e))
    {
    }

    /// <inheritdoc />
    public void Dispose() => this._unsubscribe();
}
