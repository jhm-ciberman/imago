using System;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace LifeSim.Imago.Generators.Analysis;

/// <summary>
/// Handles parsing and normalization of numeric literals for template attribute values.
/// </summary>
internal static class NumericHelper
{
    /// <summary>
    /// Determines whether the given <see cref="SpecialType"/> is a numeric type supported by the template compiler.
    /// </summary>
    /// <param name="specialType">The special type to check.</param>
    /// <returns>true if the type is a supported numeric type; otherwise, false.</returns>
    public static bool IsNumericType(SpecialType specialType)
    {
        return GetSuffix(specialType) != null;
    }

    /// <summary>
    /// Tries to parse a string as a numeric literal for the given type, normalizing it with the correct C# suffix.
    /// </summary>
    /// <param name="specialType">The target numeric type.</param>
    /// <param name="value">The raw attribute value.</param>
    /// <param name="result">The normalized C# literal (e.g., "0.5f", "300", "1.5m").</param>
    /// <returns>true if the value is a valid numeric literal for the type; otherwise, false.</returns>
    public static bool TryParseLiteral(SpecialType specialType, string value, out string result)
    {
        var suffix = GetSuffix(specialType);
        if (suffix == null)
        {
            result = "";
            return false;
        }

        var stripped = value;
        if (suffix.Length > 0 &&
            value.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            stripped = value.Substring(0, value.Length - suffix.Length);
        }

        var ic = CultureInfo.InvariantCulture;

        bool valid = specialType switch
        {
            SpecialType.System_Byte => byte.TryParse(stripped, NumberStyles.Integer, ic, out _),
            SpecialType.System_SByte => sbyte.TryParse(stripped, NumberStyles.Integer, ic, out _),
            SpecialType.System_Int16 => short.TryParse(stripped, NumberStyles.Integer, ic, out _),
            SpecialType.System_UInt16 => ushort.TryParse(stripped, NumberStyles.Integer, ic, out _),
            SpecialType.System_Int32 => int.TryParse(stripped, NumberStyles.Integer, ic, out _),
            SpecialType.System_UInt32 => uint.TryParse(stripped, NumberStyles.Integer, ic, out _),
            SpecialType.System_Int64 => long.TryParse(stripped, NumberStyles.Integer, ic, out _),
            SpecialType.System_UInt64 => ulong.TryParse(stripped, NumberStyles.Integer, ic, out _),
            SpecialType.System_Single => float.TryParse(stripped, NumberStyles.Float, ic, out _),
            SpecialType.System_Double => double.TryParse(stripped, NumberStyles.Float, ic, out _),
            SpecialType.System_Decimal => decimal.TryParse(stripped, NumberStyles.Float, ic, out _),
            _ => false,
        };

        result = valid ? stripped + suffix : "";
        return valid;
    }

    private static string? GetSuffix(SpecialType specialType)
    {
        return specialType switch
        {
            SpecialType.System_Single => "f",
            SpecialType.System_Double => "d",
            SpecialType.System_Decimal => "m",
            SpecialType.System_Int32 => "",
            SpecialType.System_UInt32 => "u",
            SpecialType.System_Int64 => "L",
            SpecialType.System_UInt64 => "uL",
            SpecialType.System_Int16 => "",
            SpecialType.System_UInt16 => "",
            SpecialType.System_Byte => "",
            SpecialType.System_SByte => "",
            _ => null,
        };
    }
}
