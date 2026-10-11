using System.Collections.Generic;
using System.Globalization;

namespace Imago.DevConsole;

/// <summary>
/// Provides context for command execution, including parsed arguments and console access.
/// </summary>
public class CommandContext
{
    private readonly Dictionary<string, string> _arguments;

    /// <summary>
    /// Gets the developer console instance.
    /// </summary>
    public DeveloperConsole Console { get; }

    /// <summary>
    /// Gets the raw argument values by name.
    /// </summary>
    public IReadOnlyDictionary<string, string> Arguments => this._arguments;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandContext"/> class.
    /// </summary>
    /// <param name="console">The developer console.</param>
    /// <param name="arguments">The parsed arguments.</param>
    public CommandContext(DeveloperConsole console, Dictionary<string, string> arguments)
    {
        this.Console = console;
        this._arguments = arguments;
    }

    /// <summary>
    /// Gets an argument value as a string.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>The argument value.</returns>
    public string GetString(string name)
    {
        return this._arguments.TryGetValue(name, out var value) ? value : string.Empty;
    }

    /// <summary>
    /// Gets an argument value as an integer.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="defaultValue">The default value if parsing fails.</param>
    /// <returns>The parsed integer value.</returns>
    public int GetInt(string name, int defaultValue = 0)
    {
        if (this._arguments.TryGetValue(name, out var value) &&
            int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets an argument value as a float.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="defaultValue">The default value if parsing fails.</param>
    /// <returns>The parsed float value.</returns>
    public float GetFloat(string name, float defaultValue = 0f)
    {
        if (this._arguments.TryGetValue(name, out var value) &&
            float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets an argument value as a boolean.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="defaultValue">The default value if parsing fails.</param>
    /// <returns>The parsed boolean value.</returns>
    public bool GetBool(string name, bool defaultValue = false)
    {
        if (!this._arguments.TryGetValue(name, out var value))
        {
            return defaultValue;
        }

        return value.ToLowerInvariant() switch
        {
            "true" or "1" or "yes" or "on" => true,
            "false" or "0" or "no" or "off" => false,
            _ => defaultValue
        };
    }

    /// <summary>
    /// Tries to get an argument value.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="value">The argument value if found.</param>
    /// <returns>True if the argument exists.</returns>
    public bool TryGetArgument(string name, out string value)
    {
        return this._arguments.TryGetValue(name, out value!);
    }

    /// <summary>
    /// Checks if an argument was provided.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <returns>True if the argument was provided.</returns>
    public bool HasArgument(string name)
    {
        return this._arguments.ContainsKey(name);
    }

    /// <summary>
    /// Resolves a boolean toggle argument. If the argument is present, parses it as a bool;
    /// otherwise inverts the current value.
    /// </summary>
    /// <param name="argName">The argument name.</param>
    /// <param name="current">The current value to invert if no argument is provided.</param>
    /// <returns>The resolved boolean value.</returns>
    public bool ResolveToggle(string argName, bool current)
    {
        return this.HasArgument(argName) ? this.GetBool(argName) : !current;
    }
}
