using System;
using System.Runtime.CompilerServices;

namespace Imago.ScenarioRunner;

/// <summary>
/// Awaitable that yields a started scenario on the game loop thread.
/// </summary>
/// <typeparam name="T">The scenario type handed back to the caller.</typeparam>
public sealed class ScenarioStart<T> : INotifyCompletion
{
    private readonly object _gate = new();
    private readonly T _result;
    private readonly LoopSynchronizationContext _loop;

    private bool _started;
    private Action? _pending;

    internal ScenarioStart(T result, LoopSynchronizationContext loop)
    {
        this._result = result;
        this._loop = loop;
    }

    /// <summary>
    /// Gets a value indicating whether the awaiter has completed. Always false, so the caller always moves to
    /// the game loop thread.
    /// </summary>
    public bool IsCompleted => false;

    /// <summary>
    /// Gets the awaiter.
    /// </summary>
    /// <returns>This instance.</returns>
    public ScenarioStart<T> GetAwaiter()
    {
        return this;
    }

    /// <inheritdoc />
    public void OnCompleted(Action continuation)
    {
        lock (this._gate)
        {
            if (!this._started)
            {
                this._pending = continuation;
                return;
            }
        }

        this._loop.Post(_ => continuation(), null);
    }

    /// <summary>
    /// Gets the scenario.
    /// </summary>
    /// <returns>The scenario that was started.</returns>
    public T GetResult()
    {
        return this._result;
    }

    internal void MarkStarted()
    {
        Action? pending;
        lock (this._gate)
        {
            this._started = true;
            pending = this._pending;
            this._pending = null;
        }

        if (pending != null)
        {
            this._loop.Post(_ => pending(), null);
        }
    }
}
