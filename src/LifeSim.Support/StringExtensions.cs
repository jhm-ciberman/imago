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
        return GetPascalCaseRegex().Replace(value, m => m.Groups[1].Value.ToUpper());
    }


    [GeneratedRegex("([a-z0-9])([A-Z])", RegexOptions.Compiled)]
    private static partial Regex GetSnakeCaseRegex();

    [GeneratedRegex("([a-z0-9])([A-Z])", RegexOptions.Compiled)]
    private static partial Regex GetPascalCaseRegex();


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
