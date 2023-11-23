using System;

namespace LifeSim.Support.Tweening;

public delegate float EasingFunction(float t);
public delegate T InterpolationFunction<T>(T start, T end, float t);

public interface ITween
{
    float Duration { get; }

    float Delay { get; }

    float CurrentTime { get; }

    bool IsFinished { get; }

    float Progress { get; }

    bool Update(float deltaTime);

    ITween AddDelay(float delay);

    ITween WithEasing(EasingFunction easing);

    ITween WithFps(float fps);

    ITween WithFrameTime(float frameTime);

    ITween WithFrameCount(int frameCount);
}

public class Tween<T> : ITween where T : struct
{
    private readonly Action<T> _setter;

    private EasingFunction _easing;

    private readonly InterpolationFunction<T> _interpolation;

    public float Duration { get; }

    public float Delay { get; private set; } = 0f;

    public float CurrentTime { get; private set; } = 0f;

    public bool IsFinished => this.CurrentTime >= this.Duration;

    public float Progress => this.CurrentTime / this.Duration;

    public T StartValue { get; }

    public T EndValue { get; }

    public float FrameTime { get; private set; } = 0f;

    public Tween(float duration, T startValue, T endValue, Action<T> setter, InterpolationFunction<T> interpolation, EasingFunction? easing = null)
    {
        this.Duration = duration;
        this.StartValue = startValue;
        this.EndValue = endValue;
        this._setter = setter;
        this._interpolation = interpolation;
        this._easing = easing ?? Easing.Quadratic.Out;
    }

    public ITween AddDelay(float delay)
    {
        this.Delay += delay;
        return this;
    }

    public ITween WithEasing(EasingFunction easing)
    {
        this._easing = easing;
        return this;
    }

    public ITween WithFps(float fps)
    {
        this.FrameTime = 1f / fps;
        return this;
    }

    public ITween WithFrameTime(float frameTime)
    {
        this.FrameTime = frameTime;
        return this;
    }

    public ITween WithFrameCount(int frameCount)
    {
        this.FrameTime = this.Duration / frameCount;
        return this;
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
                return true;
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


