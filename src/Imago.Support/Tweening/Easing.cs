using System;

namespace Imago.Support.Tweening;

// Extracted from: https://github.com/franknorton/Pleasing/blob/master/Pleasing/Easing.cs
/*
MIT License

Copyright (c) 2017 Frank Anthony Norton

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

/// <summary>
/// Defines the types of easing functions available for animations.
/// </summary>
public enum EasingType
{
    /// <summary>
    /// Linear interpolation with constant rate of change.
    /// </summary>
    Linear,

    /// <summary>
    /// Quadratic ease-in (accelerating from zero velocity).
    /// </summary>
    QuadraticIn,

    /// <summary>
    /// Quadratic ease-out (decelerating to zero velocity).
    /// </summary>
    QuadraticOut,

    /// <summary>
    /// Quadratic ease-in-out (acceleration until halfway, then deceleration).
    /// </summary>
    QuadraticInOut,

    /// <summary>
    /// Cubic ease-in (accelerating from zero velocity).
    /// </summary>
    CubicIn,

    /// <summary>
    /// Cubic ease-out (decelerating to zero velocity).
    /// </summary>
    CubicOut,

    /// <summary>
    /// Cubic ease-in-out (acceleration until halfway, then deceleration).
    /// </summary>
    CubicInOut,

    /// <summary>
    /// Quartic ease-in (accelerating from zero velocity).
    /// </summary>
    QuarticIn,

    /// <summary>
    /// Quartic ease-out (decelerating to zero velocity).
    /// </summary>
    QuarticOut,

    /// <summary>
    /// Quartic ease-in-out (acceleration until halfway, then deceleration).
    /// </summary>
    QuarticInOut,

    /// <summary>
    /// Quintic ease-in (accelerating from zero velocity).
    /// </summary>
    QuinticIn,

    /// <summary>
    /// Quintic ease-out (decelerating to zero velocity).
    /// </summary>
    QuinticOut,

    /// <summary>
    /// Quintic ease-in-out (acceleration until halfway, then deceleration).
    /// </summary>
    QuinticInOut,

    /// <summary>
    /// Sinusoidal ease-in (accelerating from zero velocity).
    /// </summary>
    SinusoidalIn,

    /// <summary>
    /// Sinusoidal ease-out (decelerating to zero velocity).
    /// </summary>
    SinusoidalOut,

    /// <summary>
    /// Sinusoidal ease-in-out (acceleration until halfway, then deceleration).
    /// </summary>
    SinusoidalInOut,

    /// <summary>
    /// Exponential ease-in (accelerating from zero velocity).
    /// </summary>
    ExponentialIn,

    /// <summary>
    /// Exponential ease-out (decelerating to zero velocity).
    /// </summary>
    ExponentialOut,

    /// <summary>
    /// Exponential ease-in-out (acceleration until halfway, then deceleration).
    /// </summary>
    ExponentialInOut,

    /// <summary>
    /// Circular ease-in (accelerating from zero velocity).
    /// </summary>
    CircularIn,

    /// <summary>
    /// Circular ease-out (decelerating to zero velocity).
    /// </summary>
    CircularOut,

    /// <summary>
    /// Circular ease-in-out (acceleration until halfway, then deceleration).
    /// </summary>
    CircularInOut,

    /// <summary>
    /// Elastic ease-in (overshooting cubic ease-in).
    /// </summary>
    ElasticIn,

    /// <summary>
    /// Elastic ease-out (overshooting cubic ease-out).
    /// </summary>
    ElasticOut,

    /// <summary>
    /// Elastic ease-in-out (overshooting cubic ease-in-out).
    /// </summary>
    ElasticInOut,

    /// <summary>
    /// Back ease-in (overshooting cubic ease-in).
    /// </summary>
    BackIn,

    /// <summary>
    /// Back ease-out (overshooting cubic ease-out).
    /// </summary>
    BackOut,

    /// <summary>
    /// Back ease-in-out (overshooting cubic ease-in-out).
    /// </summary>
    BackInOut,

    /// <summary>
    /// Bounce ease-in (exponentially decaying parabolic bounce).
    /// </summary>
    BounceIn,

    /// <summary>
    /// Bounce ease-out (exponentially decaying parabolic bounce).
    /// </summary>
    BounceOut,

    /// <summary>
    /// Bounce ease-in-out (exponentially decaying parabolic bounce).
    /// </summary>
    BounceInOut,

    /// <summary>
    /// Custom Bézier curve easing.
    /// </summary>
    Bezier
}

/// <summary>
/// Provides easing functions for smooth animations and transitions.
/// </summary>
public static class Easing
{
    /// <summary>
    /// Applies the specified easing function to a value.
    /// </summary>
    /// <param name="easingFunction">The type of easing function to apply.</param>
    /// <param name="k">The input value (typically 0.0 to 1.0).</param>
    /// <returns>The eased output value.</returns>
    public static float Ease(EasingType easingFunction, float k)
    {
        return easingFunction switch
        {
            EasingType.Linear => Linear(k),
            EasingType.QuadraticIn => Quadratic.In(k),
            EasingType.QuadraticOut => Quadratic.Out(k),
            EasingType.QuadraticInOut => Quadratic.InOut(k),
            EasingType.CubicIn => Cubic.In(k),
            EasingType.CubicOut => Cubic.Out(k),
            EasingType.CubicInOut => Cubic.InOut(k),
            EasingType.QuarticIn => Quartic.In(k),
            EasingType.QuarticOut => Quartic.Out(k),
            EasingType.QuarticInOut => Quartic.InOut(k),
            EasingType.QuinticIn => Quintic.In(k),
            EasingType.QuinticOut => Quintic.Out(k),
            EasingType.QuinticInOut => Quintic.InOut(k),
            EasingType.SinusoidalIn => Sinusoidal.In(k),
            EasingType.SinusoidalOut => Sinusoidal.Out(k),
            EasingType.SinusoidalInOut => Sinusoidal.InOut(k),
            EasingType.ExponentialIn => Exponential.In(k),
            EasingType.ExponentialOut => Exponential.Out(k),
            EasingType.ExponentialInOut => Exponential.InOut(k),
            EasingType.CircularIn => Circular.In(k),
            EasingType.CircularOut => Circular.Out(k),
            EasingType.CircularInOut => Circular.InOut(k),
            EasingType.ElasticIn => Elastic.In(k),
            EasingType.ElasticOut => Elastic.Out(k),
            EasingType.ElasticInOut => Elastic.InOut(k),
            EasingType.BackIn => Back.In(k),
            EasingType.BackOut => Back.Out(k),
            EasingType.BackInOut => Back.InOut(k),
            EasingType.BounceIn => Bounce.In(k),
            EasingType.BounceOut => Bounce.Out(k),
            EasingType.BounceInOut => Bounce.InOut(k),
            _ => Linear(k),
        };
    }

    /// <summary>
    /// Linear easing function with constant rate of change.
    /// </summary>
    /// <param name="k">The input value (0.0 to 1.0).</param>
    /// <returns>The eased output value.</returns>
    public static float Linear(float k)
    {
        return k;
    }

    /// <summary>
    /// Provides quadratic easing functions (t^2).
    /// </summary>
    public class Quadratic
    {
        /// <summary>
        /// Accelerating from zero velocity (ease-in).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return k * k;
        }

        /// <summary>
        /// Decelerating to zero velocity (ease-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            return k * (2f - k);
        }

        /// <summary>
        /// Acceleration until halfway, then deceleration (ease-in-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k;
            return -0.5f * ((k -= 1f) * (k - 2f) - 1f);
        }
    };

    /// <summary>
    /// Provides cubic easing functions (t^3).
    /// </summary>
    public class Cubic
    {
        /// <summary>
        /// Accelerating from zero velocity (ease-in).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return k * k * k;
        }

        /// <summary>
        /// Decelerating to zero velocity (ease-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            return 1f + (k -= 1f) * k * k;
        }

        /// <summary>
        /// Acceleration until halfway, then deceleration (ease-in-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k * k;
            return 0.5f * ((k -= 2f) * k * k + 2f);
        }
    };

    /// <summary>
    /// Provides quartic easing functions (t^4).
    /// </summary>
    public class Quartic
    {
        /// <summary>
        /// Accelerating from zero velocity (ease-in).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return k * k * k * k;
        }

        /// <summary>
        /// Decelerating to zero velocity (ease-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            return 1f - (k -= 1f) * k * k * k;
        }

        /// <summary>
        /// Acceleration until halfway, then deceleration (ease-in-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k * k * k;
            return -0.5f * ((k -= 2f) * k * k * k - 2f);
        }
    };

    /// <summary>
    /// Provides quintic easing functions (t^5).
    /// </summary>
    public class Quintic
    {
        /// <summary>
        /// Accelerating from zero velocity (ease-in).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return k * k * k * k * k;
        }

        /// <summary>
        /// Decelerating to zero velocity (ease-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            return 1f + (k -= 1f) * k * k * k * k;
        }

        /// <summary>
        /// Acceleration until halfway, then deceleration (ease-in-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k * k * k * k;
            return 0.5f * ((k -= 2f) * k * k * k * k + 2f);
        }
    };

    /// <summary>
    /// Provides sinusoidal easing functions based on sine/cosine curves.
    /// </summary>
    public class Sinusoidal
    {
        /// <summary>
        /// Accelerating from zero velocity using a cosine curve (ease-in).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return 1f - (float)Math.Cos(k * Math.PI / 2f);
        }

        /// <summary>
        /// Decelerating to zero velocity using a sine curve (ease-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            return (float)Math.Sin(k * Math.PI / 2f);
        }

        /// <summary>
        /// Acceleration until halfway, then deceleration using a cosine curve (ease-in-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            return 0.5f * (1f - (float)Math.Cos(Math.PI * k));
        }
    };

    /// <summary>
    /// Provides exponential easing functions based on power curves.
    /// </summary>
    public class Exponential
    {
        /// <summary>
        /// Accelerating from zero velocity using an exponential curve (ease-in).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return k == 0f ? 0f : (float)Math.Pow(1024f, k - 1f);
        }

        /// <summary>
        /// Decelerating to zero velocity using an exponential curve (ease-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            return k == 1f ? 1f : 1f - (float)Math.Pow(2f, -10f * k);
        }

        /// <summary>
        /// Acceleration until halfway, then deceleration using exponential curves (ease-in-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if (k == 0f) return 0f;
            if (k == 1f) return 1f;
            if ((k *= 2f) < 1f) return 0.5f * (float)Math.Pow(1024f, k - 1f);
            return 0.5f * (float)(-Math.Pow(2f, -10f * (k - 1f)) + 2f);
        }
    };

    /// <summary>
    /// Provides circular easing functions based on quarter-circle curves.
    /// </summary>
    public class Circular
    {
        /// <summary>
        /// Accelerating from zero velocity using a circular curve (ease-in).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return 1f - (float)Math.Sqrt(1f - k * k);
        }

        /// <summary>
        /// Decelerating to zero velocity using a circular curve (ease-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            return (float)Math.Sqrt(1f - (k -= 1f) * k);
        }

        /// <summary>
        /// Acceleration until halfway, then deceleration using circular curves (ease-in-out).
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return -0.5f * (float)(Math.Sqrt(1f - k * k) - 1);
            return 0.5f * (float)(Math.Sqrt(1f - (k -= 2f) * k) + 1f);
        }
    };

    /// <summary>
    /// Provides elastic easing functions that simulate elastic oscillations.
    /// </summary>
    public class Elastic
    {
        /// <summary>
        /// Elastic ease-in with overshooting oscillation at the beginning.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            if (k == 0) return 0;
            if (k == 1) return 1;
            return (float)-Math.Pow(2f, 10f * (k -= 1f)) * (float)Math.Sin((k - 0.1f) * (2f * Math.PI) / 0.4f);
        }

        /// <summary>
        /// Elastic ease-out with overshooting oscillation at the end.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            if (k == 0) return 0;
            if (k == 1) return 1;
            return (float)Math.Pow(2f, -10f * k) * (float)Math.Sin((k - 0.1f) * (2f * Math.PI) / 0.4f) + 1f;
        }

        /// <summary>
        /// Elastic ease-in-out with overshooting oscillations at both ends.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return -0.5f * (float)Math.Pow(2f, 10f * (k -= 1f)) * (float)Math.Sin((k - 0.1f) * (2f * Math.PI) / 0.4f);
            return (float)Math.Pow(2f, -10f * (k -= 1f)) * (float)Math.Sin((k - 0.1f) * (2f * Math.PI) / 0.4f) * 0.5f + 1f;
        }
    };

    /// <summary>
    /// Provides back easing functions that overshoot the target value before settling.
    /// </summary>
    public class Back
    {
        private static readonly float _s = 1.70158f;
        private static readonly float _s2 = 2.5949095f;

        /// <summary>
        /// Back ease-in with slight overshoot at the beginning.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return k * k * ((_s + 1f) * k - _s);
        }

        /// <summary>
        /// Back ease-out with overshoot beyond the target value.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            return (k -= 1f) * k * ((_s + 1f) * k + _s) + 1f;
        }

        /// <summary>
        /// Back ease-in-out with overshoots at both ends.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * (k * k * ((_s2 + 1f) * k - _s2));
            return 0.5f * ((k -= 2f) * k * ((_s2 + 1f) * k + _s2) + 2f);
        }
    };

    /// <summary>
    /// Provides bounce easing functions that simulate a bouncing ball effect.
    /// </summary>
    public class Bounce
    {
        /// <summary>
        /// Bounce ease-in with bouncing effect at the beginning.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float In(float k)
        {
            return 1f - Out(1f - k);
        }

        /// <summary>
        /// Bounce ease-out with bouncing effect at the end.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float Out(float k)
        {
            if (k < 1f / 2.75f)
            {
                return 7.5625f * k * k;
            }
            else if (k < 2f / 2.75f)
            {
                return 7.5625f * (k -= 1.5f / 2.75f) * k + 0.75f;
            }
            else if (k < 2.5f / 2.75f)
            {
                return 7.5625f * (k -= 2.25f / 2.75f) * k + 0.9375f;
            }
            else
            {
                return 7.5625f * (k -= 2.625f / 2.75f) * k + 0.984375f;
            }
        }

        /// <summary>
        /// Bounce ease-in-out with bouncing effects at both ends.
        /// </summary>
        /// <param name="k">The input value (0.0 to 1.0).</param>
        /// <returns>The eased output value.</returns>
        public static float InOut(float k)
        {
            if (k < 0.5f) return In(k * 2f) * 0.5f;
            return Out(k * 2f - 1f) * 0.5f + 0.5f;
        }
    };


    /// <summary>
    /// Calculates a cubic Bézier easing curve.
    /// </summary>
    /// <remarks>
    /// Implementation adapted from http://www.flong.com/texts/code/shapers_bez/
    /// </remarks>
    /// <param name="time">The input time value (0.0 to 1.0).</param>
    /// <param name="aX">The X coordinate of the first control point.</param>
    /// <param name="aY">The Y coordinate of the first control point.</param>
    /// <param name="bX">The X coordinate of the second control point.</param>
    /// <param name="bY">The Y coordinate of the second control point.</param>
    /// <returns>The eased output value.</returns>
    public static float Bezier(float time, float aX, float aY, float bX, float bY)
    {
        float y0a = 0.00f; // initial y
        float x0a = 0.00f; // initial x
        float y1a = aY;    // 1st influence y
        float x1a = aX;    // 1st influence x
        float y2a = bY;    // 2nd influence y
        float x2a = bX;    // 2nd influence x
        float y3a = 1.00f; // final y
        float x3a = 1.00f; // final x

        float A = x3a - 3 * x2a + 3 * x1a - x0a;
        float B = 3 * x2a - 6 * x1a + 3 * x0a;
        float C = 3 * x1a - 3 * x0a;
        float D = x0a;

        float E = y3a - 3 * y2a + 3 * y1a - y0a;
        float F = 3 * y2a - 6 * y1a + 3 * y0a;
        float G = 3 * y1a - 3 * y0a;
        float H = y0a;

        // Solve for t given x (using Newton-Raphelson), then solve for y given t.
        // Assume for the first guess that t = x.
        float currentt = time;
        int nRefinementIterations = 5;
        for (int i = 0; i < nRefinementIterations; i++)
        {
            float currentx = XFromT(currentt, A, B, C, D);
            float currentslope = SlopeFromT(currentt, A, B, C);
            currentt -= (currentx - time) * currentslope;
            currentt = Constrain(currentt, 0, 1);
        }

        float y = YFromT(currentt, E, F, G, H);
        return y;
    }

    /// <summary>
    /// Creates a Bézier easing function with the specified control points.
    /// </summary>
    /// <param name="aX">The X coordinate of the first control point.</param>
    /// <param name="aY">The Y coordinate of the first control point.</param>
    /// <param name="bX">The X coordinate of the second control point.</param>
    /// <param name="bY">The Y coordinate of the second control point.</param>
    /// <returns>A function that applies the Bézier easing to input values.</returns>
    public static Func<float, float> BezierFunction(float aX, float aY, float bX, float bY)
    {
        return (x) => Bezier(x, aX, aY, bX, bY);
    }

    private static float Constrain(float value, float min, float max)
    {
        return value < min ? min : value > max ? max : value;
    }

    private static float SlopeFromT(float t, float A, float B, float C)
    {
        float dtdx = 1.0f / (3.0f * A * t * t + 2.0f * B * t + C);
        return dtdx;
    }

    private static float XFromT(float t, float A, float B, float C, float D)
    {
        float x = A * (t * t * t) + B * (t * t) + C * t + D;
        return x;
    }

    private static float YFromT(float t, float E, float F, float G, float H)
    {
        float y = E * (t * t * t) + F * (t * t) + G * t + H;
        return y;
    }
}
