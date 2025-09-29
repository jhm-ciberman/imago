namespace LifeSim.Support.Numerics;

using System;
using System.Collections.Generic;
using System.Numerics;
using CommunityToolkit.Diagnostics;

/// <summary>
/// Provides extension methods for the <see cref="Random"/> class.
/// </summary>
public static class RandomExtensions
{
    /// <summary>
    /// Returns a random element from the list.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="list">The list to select from.</param>
    /// <exception cref="ArgumentException">Thrown when the list is empty.</exception>
    /// <returns>A random element from the list.</returns>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    public static T NextElement<T>(this Random random, IReadOnlyList<T> list)
    {
        Guard.IsNotEmpty(list);
        if (list.Count == 1) return list[0];
        return list[random.Next(list.Count)];
    }

    /// <summary>
    /// Returns a random <see cref="Vector2Int"/> from the specified range.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <exception cref="ArgumentException">Thrown when min is greater than or equal to max.</exception>
    /// <returns>A random <see cref="Vector2Int"/> within the specified range.</returns>
    public static Vector2Int NextVector2Int(this Random random, Vector2Int min, Vector2Int max)
    {
        if (min.X >= max.X || min.Y >= max.Y) ThrowHelper.ThrowArgumentException("Min must be less than max.");
        return new Vector2Int(random.Next(min.X, max.X), random.Next(min.Y, max.Y));
    }

    /// <summary>
    /// Returns a random <see cref="Vector2"/> from the specified range.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <exception cref="ArgumentException">Thrown when min is greater than or equal to max.</exception>
    /// <returns>A random <see cref="Vector2"/> within the specified range.</returns>
    public static Vector2 NextVector2(this Random random, Vector2 min, Vector2 max)
    {
        if (min.X >= max.X || min.Y >= max.Y) ThrowHelper.ThrowArgumentException("Min must be less than max.");
        return new Vector2(
            (float)(random.NextDouble() * (max.X - min.X) + min.X),
            (float)(random.NextDouble() * (max.Y - min.Y) + min.Y));
    }

    /// <summary>
    /// Returns a random <see cref="Vector2"/> from the specified range.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <returns>A random <see cref="Vector2"/> within the specified range.</returns>
    public static Vector2 NextVector2(this Random random, float min, float max)
    {
        return NextVector2(random, new Vector2(min), new Vector2(max));
    }

    /// <summary>
    /// Returns a random <see cref="float"/> from the specified range.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <exception cref="ArgumentException">Thrown when min is greater than or equal to max.</exception>
    /// <returns>A random <see cref="float"/> within the specified range.</returns>
    public static float NextSingle(this Random random, float min, float max)
    {
        if (min >= max) ThrowHelper.ThrowArgumentException("Min must be less than max.");
        return (float)(random.NextDouble() * (max - min) + min);
    }

    /// <summary>
    /// Returns true with the specified probability (0.0 to 1.0).
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="probability">The probability of returning true (0.0 to 1.0).</param>
    /// <returns>True if the random roll is less than the probability; otherwise, false.</returns>
    public static bool Chance(this Random random, float probability)
    {
        return random.NextDouble() < probability;
    }

    /// <summary>
    /// Rolls a number of d-sided dice and returns the total.
    /// </summary>
    /// <code>
    /// int total = random.Roll(3, d: 6); // Roll 3d6
    /// </code>
    /// <param name="random">The random number generator.</param>
    /// <param name="count">The number of dice to roll.</param>
    /// <param name="d">The number of sides on the dice.</param>
    /// <returns>The total of the rolled dice.</returns>
    public static int Roll(this Random random, int count, int d)
    {
        Guard.IsGreaterThan(count, 0);
        Guard.IsGreaterThan(d, 1);

        var total = 0;
        for (var i = 0; i < count; i++)
        {
            total += random.Next(1, d + 1);
        }

        return total;
    }

    /// <summary>
    /// Rolls a number of d-sided dice and returns the lowest result.
    /// </summary>
    /// <code>
    /// int lowest = random.RollMin(4, d: 6); // Roll 4d6 and take the lowest
    /// </code>
    /// <param name="random">The random number generator.</param>
    /// <param name="count">The number of dice to roll.</param>
    /// <param name="d">The number of sides on the dice.</param>
    /// <returns>The lowest result of the rolled dice.</returns>
    public static int RollMin(this Random random, int count, int d)
    {
        Guard.IsGreaterThan(count, 1);
        Guard.IsGreaterThan(d, 1);

        int min = int.MaxValue;
        for (var i = 0; i < count; i++)
        {
            min = Math.Min(min, random.Next(1, d + 1));
        }

        return min;
    }

    /// <summary>
    /// Rolls a number of d-sided dice and returns the highest result.
    /// </summary>
    /// <code>
    /// int highest = random.RollMax(4, d: 6); // Roll 4d6 and take the highest
    /// </code>
    /// <param name="random">The random number generator.</param>
    /// <param name="count">The number of dice to roll.</param>
    /// <param name="d">The number of sides on the dice.</param>
    /// <returns>The highest result of the rolled dice.</returns>
    public static int RollMax(this Random random, int count, int d)
    {
        Guard.IsGreaterThan(count, 1);
        Guard.IsGreaterThan(d, 1);

        int max = int.MinValue;
        for (var i = 0; i < count; i++)
        {
            max = Math.Max(max, random.Next(1, d + 1));
        }

        return max;
    }

}
