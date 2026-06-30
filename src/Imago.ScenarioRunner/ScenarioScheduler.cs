using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Imago.ScenarioRunner;

/// <summary>
/// Turns the application's frame loop into awaitable steps so scenario code can suspend until a later frame.
/// Callers advance it by calling <see cref="Tick"/> once per frame.
/// </summary>
/// <remarks>
/// Continuations resume on the thread that calls <see cref="Tick"/>, which is the game loop thread. That keeps
/// every scenario step on the loop thread, where touching view models and the renderer is safe.
/// </remarks>
internal sealed class ScenarioScheduler
{
    private readonly List<Wait> _waits = new();

    private int _frame;
    private double _elapsedSeconds;

    /// <summary>
    /// Gets the number of frames advanced so far.
    /// </summary>
    public int Frame => this._frame;

    /// <summary>
    /// Gets the total time advanced so far, in seconds.
    /// </summary>
    public double ElapsedSeconds => this._elapsedSeconds;

    /// <summary>
    /// Returns a task that completes once <paramref name="condition"/> holds on a later frame.
    /// </summary>
    /// <param name="condition">Polled once per frame until it returns true.</param>
    /// <param name="description">Label used in the timeout message.</param>
    /// <param name="timeoutSeconds">How long to wait before the task fails with a <see cref="TimeoutException"/>.</param>
    /// <returns>A task that completes when the condition holds, or faults on timeout.</returns>
    public Task WaitUntil(Func<bool> condition, string description, double timeoutSeconds)
    {
        var completion = new TaskCompletionSource();
        var wait = new Wait(
            condition: condition,
            description: description,
            deadlineSeconds: this._elapsedSeconds + timeoutSeconds,
            timeoutSeconds: timeoutSeconds,
            completion: completion);

        this._waits.Add(wait);
        return completion.Task;
    }

    /// <summary>
    /// Returns a task that completes after <paramref name="count"/> further frames.
    /// </summary>
    /// <param name="count">The number of frames to wait. Values below one are treated as one.</param>
    /// <param name="timeoutSeconds">How long to wait before the task fails with a <see cref="TimeoutException"/>.</param>
    /// <returns>A task that completes once the target frame is reached.</returns>
    public Task WaitFrames(int count, double timeoutSeconds)
    {
        int target = this._frame + Math.Max(1, count);
        return this.WaitUntil(() => this._frame >= target, $"{count} frame(s)", timeoutSeconds);
    }

    /// <summary>
    /// Returns a task that completes after <paramref name="seconds"/> further seconds of advanced time.
    /// </summary>
    /// <param name="seconds">The number of seconds to wait. Negative values are treated as zero.</param>
    /// <param name="timeoutSeconds">How long to wait before the task fails with a <see cref="TimeoutException"/>.</param>
    /// <returns>A task that completes once the target time is reached.</returns>
    public Task WaitSeconds(double seconds, double timeoutSeconds)
    {
        double target = this._elapsedSeconds + Math.Max(0.0, seconds);
        return this.WaitUntil(() => this._elapsedSeconds >= target, $"{seconds:0.##}s", timeoutSeconds);
    }

    /// <summary>
    /// Advances the scheduler by one frame, resuming waits whose condition is met and failing any that timed out.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the previous frame, in seconds.</param>
    public void Tick(double deltaTime)
    {
        this._frame++;
        this._elapsedSeconds += deltaTime;

        if (this._waits.Count == 0)
        {
            return;
        }

        // A resumed continuation runs inline and may queue the next wait, so iterate a snapshot and let
        // newly added waits settle until the following frame.
        foreach (var wait in this._waits.ToArray())
        {
            if (wait.Condition())
            {
                this._waits.Remove(wait);
                wait.Completion.SetResult();
            }
            else if (this._elapsedSeconds >= wait.DeadlineSeconds)
            {
                this._waits.Remove(wait);
                wait.Completion.SetException(new TimeoutException(
                    $"Timed out after {wait.TimeoutSeconds:0.##}s waiting for: {wait.Description}."));
            }
        }
    }

    private sealed class Wait
    {
        public Wait(
            Func<bool> condition,
            string description,
            double deadlineSeconds,
            double timeoutSeconds,
            TaskCompletionSource completion)
        {
            this.Condition = condition;
            this.Description = description;
            this.DeadlineSeconds = deadlineSeconds;
            this.TimeoutSeconds = timeoutSeconds;
            this.Completion = completion;
        }

        public Func<bool> Condition { get; }

        public string Description { get; }

        public double DeadlineSeconds { get; }

        public double TimeoutSeconds { get; }

        public TaskCompletionSource Completion { get; }
    }
}
