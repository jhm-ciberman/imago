using System;
using System.Collections.Generic;
using System.Numerics;

namespace LifeSim.Support.Tweening;

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
