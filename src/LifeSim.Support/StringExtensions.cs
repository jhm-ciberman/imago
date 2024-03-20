using System;
using System.Text.RegularExpressions;

namespace LifeSim.Support;

public static partial class StringExtensions
{
    public static string ToSnakeCase(this string value)
    {
        return GetSnakeCaseRegex().Replace(value, "$1_$2").ToLower();
    }

    public static string ToPascalCase(this string value)
    {
        // from snake_case to PascalCase
        return GetPascalCaseRegex().Replace(value, m => m.Groups[1].Value.ToUpper());
    }


    [GeneratedRegex("([a-z0-9])([A-Z])", RegexOptions.Compiled)]
    private static partial Regex GetSnakeCaseRegex();

    [GeneratedRegex("(?:^|_)([a-z])")]
    private static partial Regex GetPascalCaseRegex();

    /// <summary>
    /// Gets a deterministic hash code for the string.
    /// </summary>
    /// <remarks>
    /// This method is used to get a deterministic hash code for a string. The default implementation of <see cref="string.GetHashCode()"/>
    /// is not deterministic and can change between different runs of the program. This method is used to get a deterministic hash code.
    /// This means the same string will always return the same hash code.
    /// </remarks>
    /// <param name="str">The string to get the hash code for.</param>
    /// <returns>The deterministic hash code for the string.</returns>
    public static int GetDeterministicHashCode(this string str)
    {
        // https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
        unchecked
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1)
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }


    public static int LevenshteinDistance(this string source, string target)
    {
        if (string.IsNullOrEmpty(source))
        {
            if (string.IsNullOrEmpty(target))
            {
                return 0;
            }

            return target.Length;
        }

        if (string.IsNullOrEmpty(target))
        {
            return source.Length;
        }

        if (source.Length > target.Length)
        {
            (source, target) = (target, source);
        }

        var m = target.Length;
        var n = source.Length;
        var distance = new int[2, m + 1];

        for (var j = 1; j <= m; j++)
        {
            distance[0, j] = j;
        }

        var currentRow = 0;
        for (var i = 1; i <= n; ++i)
        {
            currentRow = i & 1;
            distance[currentRow, 0] = i;
            var previousRow = currentRow ^ 1;
            for (var j = 1; j <= m; j++)
            {
                var cost = (target[j - 1] == source[i - 1] ? 0 : 1);
                distance[currentRow, j] = Math.Min(
                    Math.Min(distance[previousRow, j] + 1, distance[currentRow, j - 1] + 1),
                    distance[previousRow, j - 1] + cost);
            }
        }

        return distance[currentRow, m];
    }

}
