namespace LifeSim.Support.Numerics;

using System;
using System.Collections.Generic;

public static class RandomExtensions
{
    /// <summary>
    /// Returns a random element from the list.
    /// </summary>
    /// <param name="random">The random number generator.</param>
    /// <param name="list">The list to select from.</param>
    /// <exception cref="ArgumentException">Thrown when the list is empty.</exception>
    /// <typeparam name="T">The type of the elements in the list.</typeparam>
    public static T NextElement<T>(this Random random, IList<T> list)
    {
        if (list.Count == 0) throw new ArgumentException("List cannot be empty.", nameof(list));
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
        if (min.X >= max.X || min.Y >= max.Y) throw new ArgumentException("Min must be less than max.");
        return new Vector2Int(random.Next(min.X, max.X), random.Next(min.Y, max.Y));
    }
}
