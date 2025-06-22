using System;
using System.Collections.Generic;

namespace LifeSim.Imago.Utilities;

public class DisposeCollector
{
    private readonly List<IDisposable> _disposables = new List<IDisposable>();

    public void Add(IDisposable disposable)
    {
        this._disposables.Add(disposable);
    }

    public void Add(IDisposable first, IDisposable second)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
    }

    public void Add(IDisposable first, IDisposable second, IDisposable third)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
    }

    public void Add(IDisposable first, IDisposable second, IDisposable third, IDisposable fourth)
    {
        this._disposables.Add(first);
        this._disposables.Add(second);
        this._disposables.Add(third);
        this._disposables.Add(fourth);
    }

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

    public void Add<T>(T[] array) where T : IDisposable
    {
        foreach (T item in array)
        {
            this._disposables.Add(item);
        }
    }

    public void Remove(IDisposable disposable)
    {
        if (!this._disposables.Remove(disposable))
            throw new InvalidOperationException("Unable to untrack " + disposable + ". It was not previously tracked.");
    }

    public void DisposeAll()
    {
        foreach (IDisposable disposable in this._disposables)
        {
            disposable.Dispose();
        }

        this._disposables.Clear();
    }
}
