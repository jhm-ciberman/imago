using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LifeSim.Support;

/// <summary>
/// Represents a catalog of identifiable objects.
/// </summary>
/// <typeparam name="TElement">The type of the elements in the catalog.</typeparam>
public abstract class Catalog<TElement> where TElement : class, IIdentifiable
{
    private readonly Dictionary<string, TElement> _elements = new Dictionary<string, TElement>();

    protected virtual string GetElementNotFoundMessage(string identifier)
    {
        return $"Element with identifier '{identifier}' not found.";
    }

    protected virtual string GetElementAlreadyExistsMessage(string identifier)
    {
        return $"Element with identifier '{identifier}' already exists.";
    }

    protected virtual string GetElementCastErrorMessage(string identifier, Type expectedType, Type actualType)
    {
        return $"Element with identifier '{identifier}' is not of type '{expectedType.Name}' but of type '{actualType.Name}'.";
    }

    protected virtual void OnElementAdded(TElement element)
    {
        // This can be overridden to do something when an element is added to the catalog.
    }

    /// <summary>
    /// Gets the number of elements in this catalog.
    /// </summary>
    public int Count => this._elements.Count;

    /// <summary>
    /// Gets the element with the specified identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the element to get.</param>
    /// <returns>The element with the specified identifier.</returns>
    public IIdentifiable this[string identifier] => this.Get(identifier);

    /// <summary>
    /// Adds an element to this catalog.
    /// </summary>
    /// <param name="element">The element to add.</param>
    public void Add(TElement element)
    {
        if (this._elements.ContainsKey(element.Identifier))
        {
            throw new InvalidOperationException(this.GetElementAlreadyExistsMessage(element.Identifier));
        }

        this._elements.Add(element.Identifier, element);

        this.OnElementAdded(element);
    }

    /// <summary>
    /// Adds a range of elements to this catalog.
    /// </summary>
    /// <param name="elements">The elements to add.</param>
    public void AddRange(IEnumerable<TElement> elements)
    {
        foreach (var element in elements)
        {
            this.Add(element);
        }
    }

    /// <summary>
    /// Finds an element in this catalog.
    /// </summary>
    /// <param name="identifier">The identifier of the element to find.</param>
    /// <returns>The element with the specified identifier.</returns>
    public TElement Get(string identifier)
    {
        if (!this._elements.TryGetValue(identifier, out var element))
        {
            throw new KeyNotFoundException(this.GetElementNotFoundMessage(identifier));
        }

        return element;
    }

    /// <summary>
    /// Finds an element in this catalog ensuring that it is of the specified type.
    /// </summary>
    /// <param name="identifier">The identifier of the element to find.</param>
    /// <param name="type">The type of the element to find.</param>
    /// <returns>The element with the specified identifier.</returns>
    public TElement Get(string identifier, Type type)
    {
        var element = this.Get(identifier);

        if (!type.IsInstanceOfType(element))
        {
            throw new InvalidCastException(this.GetElementCastErrorMessage(identifier, type, element.GetType()));
        }

        return element;
    }

    /// <summary>
    /// Finds an element in this catalog ensuring that it is of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the element to find.</typeparam>
    /// <param name="identifier">The identifier of the element to find.</param>
    /// <returns>The element with the specified identifier.</returns>
    public T Get<T>(string identifier) where T : TElement
    {
        var element = this.Get(identifier);

        if (element is not T typedElement)
        {
            throw new InvalidCastException(this.GetElementCastErrorMessage(identifier, typeof(T), element.GetType()));
        }

        return typedElement;
    }

    /// <summary>
    /// Gets all elements of the specified type in this catalog.
    /// </summary>
    /// <typeparam name="T">The type of the elements to get.</typeparam>
    /// <returns>All elements of the specified type.</returns>
    public IEnumerable<T> GetAll<T>() where T : TElement
    {
        return this._elements.Values.Where(element => element is T).Cast<T>();
    }

    /// <summary>
    /// Returns the first element of the specified type in this catalog.
    /// </summary>
    /// <typeparam name="T">The type of the element to get.</typeparam>
    /// <returns>The first element of the specified type.</returns>
    public T First<T>() where T : TElement
    {
        return this.GetAll<T>().First();
    }

    /// <summary>
    /// Gets all elements in this catalog as an enumerable.
    /// </summary>
    /// <returns>All elements in this catalog.</returns>
    public IEnumerable<TElement> GetAll()
    {
        return this._elements.Values;
    }

}
