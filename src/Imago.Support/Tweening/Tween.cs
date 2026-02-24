using System;

namespace Imago.Support.Tweening;

/// <summary>
/// Represents a function that applies easing to a normalized time value.
/// </summary>
/// <param name="t">The normalized time value (0.0 to 1.0).</param>
/// <returns>The eased value.</returns>
public delegate float EasingFunction(float t);

/// <summary>
/// Represents a function that interpolates between two values.
/// </summary>
/// <typeparam name="T">The type of values to interpolate.</typeparam>
/// <param name="start">The starting value.</param>
/// <param name="end">The ending value.</param>
/// <param name="t">The interpolation factor (0.0 to 1.0).</param>
/// <returns>The interpolated value.</returns>
public delegate T InterpolationFunction<T>(T start, T end, float t);

/// <summary>
/// Defines the contract for a tween that can animate values over time.
/// </summary>
public interface ITween
{
    /// <summary>
    /// Gets the total duration of the tween in seconds.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Gets the delay before the tween starts in seconds.
    /// </summary>
    public float Delay { get; }

    /// <summary>
    /// Gets the current time position of the tween in seconds.
    /// </summary>
    public float CurrentTime { get; }

    /// <summary>
    /// Gets a value indicating whether the tween has finished.
    /// </summary>
    public bool IsFinished { get; }

    /// <summary>
    /// Gets the current progress of the tween (0.0 to 1.0).
    /// </summary>
    public float Progress { get; }

    /// <summary>
    /// Updates the tween with the given delta time.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    /// <returns>true if the tween is still running; false if it has finished.</returns>
    public bool Update(float deltaTime);

    /// <summary>
    /// Adds a delay before the tween starts.
    /// </summary>
    /// <param name="delay">The delay in seconds.</param>
    /// <returns>This tween instance for method chaining.</returns>
    public ITween AddDelay(float delay);

    /// <summary>
    /// Sets the easing function for the tween.
    /// </summary>
    /// <param name="easing">The easing function to use.</param>
    /// <returns>This tween instance for method chaining.</returns>
    public ITween WithEasing(EasingFunction easing);

    /// <summary>
    /// Sets the frame rate for the tween animation.
    /// </summary>
    /// <param name="fps">The frames per second.</param>
    /// <returns>This tween instance for method chaining.</returns>
    public ITween WithFps(float fps);

    /// <summary>
    /// Sets the time between frames for the tween animation.
    /// </summary>
    /// <param name="frameTime">The time between frames in seconds.</param>
    /// <returns>This tween instance for method chaining.</returns>
    public ITween WithFrameTime(float frameTime);

    /// <summary>
    /// Sets the total number of frames for the tween animation.
    /// </summary>
    /// <param name="frameCount">The total number of frames.</param>
    /// <returns>This tween instance for method chaining.</returns>
    public ITween WithFrameCount(int frameCount);

    /// <summary>
    /// Immediately stops the tween and sets it to its end value.
    /// </summary>
    public void Stop();
}

/// <summary>
/// A generic tween implementation that animates values of type <typeparamref name="T"/> over time.
/// </summary>
/// <typeparam name="T">The type of value to animate (must be a struct).</typeparam>
public class Tween<T> : ITween where T : struct
{
    private readonly Action<T> _setter;

    private EasingFunction _easing;

    private readonly InterpolationFunction<T> _interpolation;

    /// <summary>
    /// Gets the total duration of the tween in seconds.
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Gets or sets the delay before the tween starts in seconds.
    /// </summary>
    public float Delay { get; private set; } = 0f;

    /// <summary>
    /// Gets the current time position of the tween in seconds.
    /// </summary>
    public float CurrentTime { get; private set; } = 0f;

    /// <summary>
    /// Gets a value indicating whether the tween has finished.
    /// </summary>
    public bool IsFinished => this.CurrentTime >= this.Duration;

    /// <summary>
    /// Gets the current progress of the tween (0.0 to 1.0).
    /// </summary>
    public float Progress => this.CurrentTime / this.Duration;

    /// <summary>
    /// Gets the starting value of the tween.
    /// </summary>
    public T StartValue { get; }

    /// <summary>
    /// Gets the ending value of the tween.
    /// </summary>
    public T EndValue { get; }

    /// <summary>
    /// Gets the time between animation frames in seconds.
    /// </summary>
    public float FrameTime { get; private set; } = 0f;

    /// <summary>
    /// Initializes a new instance of the <see cref="Tween{T}"/> class.
    /// </summary>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="startValue">The starting value.</param>
    /// <param name="endValue">The ending value.</param>
    /// <param name="setter">The action to call with interpolated values.</param>
    /// <param name="interpolation">The interpolation function to use.</param>
    /// <param name="easing">The easing function to use (defaults to Quadratic.Out).</param>
    public Tween(float duration, T startValue, T endValue, Action<T> setter, InterpolationFunction<T> interpolation, EasingFunction? easing = null)
    {
        this.Duration = duration;
        this.StartValue = startValue;
        this.EndValue = endValue;
        this._setter = setter;
        this._interpolation = interpolation;
        this._easing = easing ?? Easing.Quadratic.Out;
    }

    /// <inheritdoc />
    public ITween AddDelay(float delay)
    {
        this.Delay += delay;
        return this;
    }

    /// <inheritdoc />
    public ITween WithEasing(EasingFunction easing)
    {
        this._easing = easing;
        return this;
    }

    /// <inheritdoc />
    public ITween WithFps(float fps)
    {
        this.FrameTime = 1f / fps;
        return this;
    }

    /// <inheritdoc />
    public ITween WithFrameTime(float frameTime)
    {
        this.FrameTime = frameTime;
        return this;
    }

    /// <inheritdoc />
    public ITween WithFrameCount(int frameCount)
    {
        this.FrameTime = this.Duration / frameCount;
        return this;
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (this.IsFinished) return;
        this.CurrentTime = this.Duration;
        this._setter(this.EndValue);
    }

    /// <summary>
    /// Update the tween and returns true if the tween is still running.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    /// <returns>True if the tween is still running, false otherwise.</returns>
    public bool Update(float deltaTime)
    {
        if (this.Delay > 0f)
        {
            this.Delay -= deltaTime;

            if (this.Delay > 0f)
            {
                return true;
            }
            else
            {
                deltaTime = -this.Delay;
                this.Delay = 0f;
            }
        }

        this.CurrentTime += deltaTime;
        if (this.CurrentTime > this.Duration)
        {
            this.CurrentTime = this.Duration;
            this._setter(this.EndValue);
            return false;
        }

        // We need to update the value. We want to floor the current time to the nearest frame time.

        float currentTime = this.CurrentTime;

        if (this.FrameTime > 0f)
            currentTime = (float)Math.Floor(currentTime / this.FrameTime) * this.FrameTime;

        float t = this._easing(currentTime / this.Duration);
        T value = this._interpolation(this.StartValue, this.EndValue, t);
        this._setter(value);
        return true;
    }
}


