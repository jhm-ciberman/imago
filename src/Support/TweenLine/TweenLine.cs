using System;
using System.Collections.Generic;
using System.Numerics;

namespace Support.TweenLine;

public delegate float EasingFunction(float t);
public delegate T InterpolationFunction<T>(T start, T end, float t);

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
        {
            currentTime = (float)Math.Floor(currentTime / this.FrameTime) * this.FrameTime;
        }

        float t = this._easing(currentTime / this.Duration);
        T value = this._interpolation(this.StartValue, this.EndValue, t);
        this._setter(value);
        return true;
    }
}

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


public class TweenLine
{
    private readonly List<ITween> _tweens = new();

    public ITween AddTween(ITween tween)
    {
        this._tweens.Add(tween);
        return tween;
    }

    public ITween FromTo(float duration, Quaternion from, Quaternion to, Action<Quaternion> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<Quaternion>(duration, from, to, setter, Quaternion.Slerp, easing));
    }

    public ITween FromTo(float duration, Vector3 from, Vector3 to, Action<Vector3> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<Vector3>(duration, from, to, setter, Vector3.Lerp, easing));
    }

    public ITween FromTo(float duration, float from, float to, Action<float> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<float>(duration, from, to, setter, Lerp, easing));
    }

    private static float Lerp(float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    public void Update(float deltaTime)
    {
        for (int i = 0; i < this._tweens.Count; i++)
        {
            if (!this._tweens[i].Update(deltaTime))
            {
                this._tweens.RemoveAt(i);
                i--;
            }
        }
    }
}
