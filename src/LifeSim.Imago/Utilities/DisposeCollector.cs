using System;
using System.Collections.Generic;

namespace LifeSim.Imago.Utilities;

/// <summary>
/// A utility class for collecting and disposing of multiple <see cref="IDisposable"/> objects at once.
/// </summary>
public class DisposeCollector
{
    private readonly List<IDisposable> _disposables = new List<IDisposable>();

    /// <summary>
    /// Adds a disposable object to the collector.
    /// </summary>
    /// <param name="disposable">The disposable object to add.</param>
    public void Add(IDisposable disposable)
    {
        this._disposables.Add(disposable);
    }

    /// <summary>
    /// Adds two disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    public void Add(IDisposable first, IDisposable second)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
    }

    /// <summary>
    /// Adds three disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    /// <param name="third">The third disposable object.</param>
    public void Add(IDisposable first, IDisposable second, IDisposable third)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
    }

    /// <summary>
    /// Adds four disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    /// <param name="third">The third disposable object.</param>
    /// <param name="fourth">The fourth disposable object.</param>
    public void Add(IDisposable first, IDisposable second, IDisposable third, IDisposable fourth)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
        this._disposables.Add(fourth);
    }

    /// <summary>
    /// Adds five disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    /// <param name="third">The third disposable object.</param>
    /// <param name="fourth">The fourth disposable object.</param>
    /// <param name="fifth">The fifth disposable object.</param>
    public void Add(
        IDisposable first,
        IDisposable second,
        IDisposable third,
        IDisposable fourth,
        IDisposable fifth)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
        this._disposables.Add(fourth);
        this._disposables.Add(fifth);
    }

    /// <summary>
    /// Adds six disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    /// <param name="third">The third disposable object.</param>
    /// <param name="fourth">The fourth disposable object.</param>
    /// <param name="fifth">The fifth disposable object.</param>
    /// <param name="sixth">The sixth disposable object.</param>
    public void Add(
        IDisposable first,
        IDisposable second,
        IDisposable third,
        IDisposable fourth,
        IDisposable fifth,
        IDisposable sixth)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
        this._disposables.Add(fourth);
        this._disposables.Add(fifth);
        this._disposables.Add(sixth);
    }

    /// <summary>
    /// Adds seven disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    /// <param name="third">The third disposable object.</param>
    /// <param name="fourth">The fourth disposable object.</param>
    /// <param name="fifth">The fifth disposable object.</param>
    /// <param name="sixth">The sixth disposable object.</param>
    /// <param name="seventh">The seventh disposable object.</param>
    public void Add(
        IDisposable first,
        IDisposable second,
        IDisposable third,
        IDisposable fourth,
        IDisposable fifth,
        IDisposable sixth,
        IDisposable seventh)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
        this._disposables.Add(fourth);
        this._disposables.Add(fifth);
        this._disposables.Add(sixth);
        this._disposables.Add(seventh);
    }

    /// <summary>
    /// Adds eight disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    /// <param name="third">The third disposable object.</param>
    /// <param name="fourth">The fourth disposable object.</param>
    /// <param name="fifth">The fifth disposable object.</param>
    /// <param name="sixth">The sixth disposable object.</param>
    /// <param name="seventh">The seventh disposable object.</param>
    /// <param name="eighth">The eighth disposable object.</param>
    public void Add(
        IDisposable first,
        IDisposable second,
        IDisposable third,
        IDisposable fourth,
        IDisposable fifth,
        IDisposable sixth,
        IDisposable seventh,
        IDisposable eighth)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
        this._disposables.Add(fourth);
        this._disposables.Add(fifth);
        this._disposables.Add(sixth);
        this._disposables.Add(seventh);
        this._disposables.Add(eighth);
    }

    /// <summary>
    /// Adds nine disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    /// <param name="third">The third disposable object.</param>
    /// <param name="fourth">The fourth disposable object.</param>
    /// <param name="fifth">The fifth disposable object.</param>
    /// <param name="sixth">The sixth disposable object.</param>
    /// <param name="seventh">The seventh disposable object.</param>
    /// <param name="eighth">The eighth disposable object.</param>
    /// <param name="ninth">The ninth disposable object.</param>
    public void Add(
        IDisposable first,
        IDisposable second,
        IDisposable third,
        IDisposable fourth,
        IDisposable fifth,
        IDisposable sixth,
        IDisposable seventh,
        IDisposable eighth,
        IDisposable ninth)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
        this._disposables.Add(fourth);
        this._disposables.Add(fifth);
        this._disposables.Add(sixth);
        this._disposables.Add(seventh);
        this._disposables.Add(eighth);
        this._disposables.Add(ninth);
    }

    /// <summary>
    /// Adds ten disposable objects to the collector.
    /// </summary>
    /// <param name="first">The first disposable object.</param>
    /// <param name="second">The second disposable object.</param>
    /// <param name="third">The third disposable object.</param>
    /// <param name="fourth">The fourth disposable object.</param>
    /// <param name="fifth">The fifth disposable object.</param>
    /// <param name="sixth">The sixth disposable object.</param>
    /// <param name="seventh">The seventh disposable object.</param>
    /// <param name="eighth">The eighth disposable object.</param>
    /// <param name="ninth">The ninth disposable object.</param>
    /// <param name="tenth">The tenth disposable object.</param>
    public void Add(
        IDisposable first,
        IDisposable second,
        IDisposable third,
        IDisposable fourth,
        IDisposable fifth,
        IDisposable sixth,
        IDisposable seventh,
        IDisposable eighth,
        IDisposable ninth,
        IDisposable tenth)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
        this._disposables.Add(fourth);
        this._disposables.Add(fifth);
        this._disposables.Add(sixth);
        this._disposables.Add(seventh);
        this._disposables.Add(eighth);
        this._disposables.Add(ninth);
        this._disposables.Add(tenth);
    }

    /// <summary>
    /// Adds an array of disposable objects to the collector.
    /// </summary>
    /// <typeparam name="T">The type of the disposable objects.</typeparam>
    /// <param name="array">The array of disposable objects.</param>
    public void Add<T>(T[] array) where T : IDisposable
    {
        foreach (T item in array)
        {
            this._disposables.Add(item);
        }
    }

    /// <summary>
    /// Removes a disposable object from the collector.
    /// </summary>
    /// <param name="disposable">The disposable object to remove.</param>
    public void Remove(IDisposable disposable)
    {
        if (!this._disposables.Remove(disposable))
            throw new InvalidOperationException("Unable to untrack " + disposable + ". It was not previously tracked.");
    }

    /// <summary>
    /// Disposes all collected objects and clears the collector.
    /// </summary>
    public void DisposeAll()
    {
        foreach (IDisposable disposable in this._disposables)
        {
            disposable.Dispose();
        }

        this._disposables.Clear();
    }
}
