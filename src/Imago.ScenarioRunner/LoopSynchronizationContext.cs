using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Imago.ScenarioRunner;

/// <summary>
/// Resumes awaited continuations on the game loop thread, one batch per frame.
/// </summary>
internal sealed class LoopSynchronizationContext : SynchronizationContext
{
    private readonly ConcurrentQueue<(SendOrPostCallback Callback, object? State)> _queue = new();

    /// <inheritdoc />
    public override void Post(SendOrPostCallback d, object? state)
    {
        this._queue.Enqueue((d, state));
    }

    /// <inheritdoc />
    public override void Send(SendOrPostCallback d, object? state)
    {
        throw new NotSupportedException("Synchronous sends to the game loop are not supported.");
    }

    /// <summary>
    /// Runs every continuation posted so far. Ones posted while running wait for the next call.
    /// </summary>
    public void Drain()
    {
        int count = this._queue.Count;
        for (int i = 0; i < count && this._queue.TryDequeue(out var item); i++)
        {
            item.Callback(item.State);
        }
    }
}
