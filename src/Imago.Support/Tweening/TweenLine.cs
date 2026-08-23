using System;
using System.Collections.Generic;
using System.Numerics;
using Imago.Support.Drawing;

namespace Imago.Support.Tweening;

/// <summary>
/// Manages a collection of tweens that can be updated together.
/// </summary>
public class TweenLine
{
    private readonly List<ITween> _tweens = new();

    /// <summary>
    /// Adds a tween to the collection.
    /// </summary>
    /// <param name="tween">The tween to add.</param>
    /// <returns>The added tween for method chaining.</returns>
    public ITween AddTween(ITween tween)
    {
        this._tweens.Add(tween);
        return tween;
    }

    /// <summary>
    /// Creates and adds a quaternion tween from one value to another.
    /// </summary>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="from">The starting quaternion value.</param>
    /// <param name="to">The ending quaternion value.</param>
    /// <param name="setter">The action to call with interpolated values.</param>
    /// <param name="easing">The easing function to use (optional).</param>
    /// <returns>The created tween.</returns>
    public ITween FromTo(float duration, Quaternion from, Quaternion to, Action<Quaternion> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<Quaternion>(duration, from, to, setter, Quaternion.Slerp, easing));
    }

    /// <summary>
    /// Creates and adds a Vector2 tween from one value to another.
    /// </summary>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="from">The starting Vector2 value.</param>
    /// <param name="to">The ending Vector2 value.</param>
    /// <param name="setter">The action to call with interpolated values.</param>
    /// <param name="easing">The easing function to use (optional).</param>
    /// <returns>The created tween.</returns>
    public ITween FromTo(float duration, Vector2 from, Vector2 to, Action<Vector2> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<Vector2>(duration, from, to, setter, Vector2.Lerp, easing));
    }

    /// <summary>
    /// Creates and adds a Vector3 tween from one value to another.
    /// </summary>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="from">The starting Vector3 value.</param>
    /// <param name="to">The ending Vector3 value.</param>
    /// <param name="setter">The action to call with interpolated values.</param>
    /// <param name="easing">The easing function to use (optional).</param>
    /// <returns>The created tween.</returns>
    public ITween FromTo(float duration, Vector3 from, Vector3 to, Action<Vector3> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<Vector3>(duration, from, to, setter, Vector3.Lerp, easing));
    }

    /// <summary>
    /// Creates and adds a float tween from one value to another.
    /// </summary>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="from">The starting float value.</param>
    /// <param name="to">The ending float value.</param>
    /// <param name="setter">The action to call with interpolated values.</param>
    /// <param name="easing">The easing function to use (optional).</param>
    /// <returns>The created tween.</returns>
    public ITween FromTo(float duration, float from, float to, Action<float> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<float>(duration, from, to, setter, Lerp, easing));
    }

    /// <summary>
    /// Creates and adds a color tween from one value to another.
    /// </summary>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="from">The starting color.</param>
    /// <param name="to">The ending color.</param>
    /// <param name="setter">The action to call with interpolated values.</param>
    /// <param name="easing">The easing function to use (optional).</param>
    /// <returns>The created tween.</returns>
    public ITween FromTo(float duration, Color from, Color to, Action<Color> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<Color>(duration, from, to, setter, Color.Lerp, easing));
    }

    /// <summary>
    /// Creates and adds a floating point color tween from one value to another.
    /// </summary>
    /// <param name="duration">The duration of the tween in seconds.</param>
    /// <param name="from">The starting color.</param>
    /// <param name="to">The ending color.</param>
    /// <param name="setter">The action to call with interpolated values.</param>
    /// <param name="easing">The easing function to use (optional).</param>
    /// <returns>The created tween.</returns>
    public ITween FromTo(float duration, ColorF from, ColorF to, Action<ColorF> setter, EasingFunction? easing = null)
    {
        return this.AddTween(new Tween<ColorF>(duration, from, to, setter, ColorF.Lerp, easing));
    }

    private static float Lerp(float start, float end, float t)
    {
        return start + (end - start) * t;
    }

    /// <summary>
    /// Updates all tweens in the collection and removes finished ones.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    public void Update(float deltaTime)
    {
        for (int i = 0; i < this._tweens.Count; i++)
        {
            var tween = this._tweens[i];
            if (tween.IsFinished || !tween.Update(deltaTime))
            {
                this._tweens.RemoveAt(i);
                i--;
            }
        }
    }
}
