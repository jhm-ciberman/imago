using System;
using System.Collections.Generic;
using System.Linq;
using Imago.Support.Numerics;

namespace Imago.Support;

/// <summary>
/// Provides extension methods for LINQ operations.
/// </summary>
public static class LinqExtensions
{
    /// <summary>
    /// Invokes the specified action with the current instance and returns the instance.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="source">The instance to invoke the action on.</param>
    /// <param name="action">The action to invoke with the current instance.</param>
    /// <returns>The current instance for method chaining.</returns>
    public static T Tap<T>(this T source, Action<T> action)
    {
        action(source);
        return source;
    }

    /// <summary>
    /// Invokes the specified action with the current instance if the condition is true, and returns the instance.
    /// </summary>
    /// <typeparam name="T">The type of the instance.</typeparam>
    /// <param name="source">The instance to potentially invoke the action on.</param>
    /// <param name="condition">If true, the action will be invoked; otherwise, it will be skipped.</param>
    /// <param name="action">The action to invoke with the current instance if the condition is true.</param>
    /// <returns>The current instance for method chaining.</returns>
    public static T TapIf<T>(this T source, bool condition, Action<T> action)
    {
        if (condition) action(source);
        return source;
    }

    /// <summary>
    /// Randomly shuffles the elements of the source enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source enumerable.</typeparam>
    /// <param name="source">The source enumerable to shuffle.</param>
    /// <param name="random">An optional random number generator. If not provided, <see cref="Random.Shared"/> is used.</param>
    /// <returns>A new enumerable with the elements of the source shuffled in random order.</returns>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random? random = null)
    {
        random ??= Random.Shared;
        return source.OrderBy(_ => random.Next());
    }

    /// <summary>
    /// Returns a new enumerable containing elements from the source enumerable, each included with the specified probability.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source enumerable.</typeparam>
    /// <param name="source">The source enumerable to take elements from.</param>
    /// <param name="chance">The probability of each element being included.</param>
    /// <param name="random">An optional random number generator. If not provided, <see cref="Random.Shared"/> is used.</param>
    /// <returns>A new enumerable containing elements from the source enumerable, each included with the specified probability.</returns>
    public static IEnumerable<T> Sample<T>(this IEnumerable<T> source, double chance, Random? random = null)
    {
        random ??= Random.Shared;
        return source.Where(_ => random.NextDouble() < chance);
    }

    /// <summary>
    /// Returns a new enumerable containing a specified number of random elements from the source enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source enumerable.</typeparam>
    /// <param name="source">The source enumerable to take elements from.</param>
    /// <param name="count">The number of random elements to take.</param>
    /// <param name="random">An optional random number generator. If not provided, <see cref="Random.Shared"/> is used.</param>
    /// <returns>A new enumerable containing the specified number of random elements from the source enumerable.</returns>
    public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> source, int count, Random? random = null)
    {
        random ??= Random.Shared;
        return source.Shuffle(random).Take(count);
    }

    /// <summary>
    /// Picks one random element from the source enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source enumerable.</typeparam>
    /// <param name="source">The source enumerable to take elements from.</param>
    /// <param name="random">An optional random number generator. If not provided, <see cref="Random.Shared"/> is used.</param>
    /// <returns>A random element from the source enumerable.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the source enumerable is empty.</exception>
    public static T PickOneRandom<T>(this IEnumerable<T> source, Random? random = null)
    {
        random ??= Random.Shared;
        var list = source as IReadOnlyList<T> ?? source.ToList();
        if (list.Count == 0) throw new InvalidOperationException("Sequence contains no elements");
        return random.NextElement(list);
    }
}
