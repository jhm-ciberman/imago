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
}
