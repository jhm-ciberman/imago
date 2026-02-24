using System;
using System.Collections.Generic;
using System.Linq;

namespace Imago.DevConsole;

/// <summary>
/// Manages registration and lookup of console commands.
/// </summary>
public class CommandRegistry
{
    private readonly List<ConsoleCommand> _commands = [];

    /// <summary>
    /// Gets all registered commands.
    /// </summary>
    public IReadOnlyList<ConsoleCommand> Commands => this._commands;

    /// <summary>
    /// Registers a command.
    /// </summary>
    /// <param name="command">The command to register.</param>
    public void Register(ConsoleCommand command)
    {
        this._commands.Add(command);
    }

    /// <summary>
    /// Registers a command by type.
    /// </summary>
    /// <typeparam name="T">The command type.</typeparam>
    public void Register<T>()
        where T : ConsoleCommand, new()
    {
        this._commands.Add(new T());
    }

    /// <summary>
    /// Starts building an inline command with the given name.
    /// </summary>
    /// <param name="names">The command name segments (e.g., "time", "speed").</param>
    /// <returns>A builder to configure the command.</returns>
    public CommandBuilder Register(params string[] names)
    {
        return new CommandBuilder(this, names);
    }

    /// <summary>
    /// Finds a command matching the given input tokens.
    /// </summary>
    /// <param name="tokens">The input tokens (command parts and arguments).</param>
    /// <param name="command">The matched command if found.</param>
    /// <param name="argumentTokens">The remaining tokens that are arguments.</param>
    /// <returns>True if a command was found.</returns>
    public bool TryFindCommand(
        string[] tokens,
        out ConsoleCommand? command,
        out string[] argumentTokens)
    {
        command = null;
        argumentTokens = [];

        if (tokens.Length == 0)
        {
            return false;
        }

        ConsoleCommand? bestMatch = null;
        int bestMatchLength = 0;

        foreach (var cmd in this._commands)
        {
            int matchLength = this.GetMatchLength(cmd.Names, tokens);
            if (matchLength > bestMatchLength)
            {
                bestMatch = cmd;
                bestMatchLength = matchLength;
            }
        }

        if (bestMatch != null)
        {
            command = bestMatch;
            argumentTokens = tokens.Skip(bestMatchLength).ToArray();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all commands that start with the given prefix.
    /// </summary>
    /// <param name="prefix">The command prefix (e.g., "time").</param>
    /// <returns>Commands matching the prefix.</returns>
    public IEnumerable<ConsoleCommand> GetCommandsWithPrefix(string prefix)
    {
        var prefixParts = prefix.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return this._commands.Where(cmd =>
        {
            if (prefixParts.Length > cmd.Names.Count)
            {
                return false;
            }

            for (int i = 0; i < prefixParts.Length; i++)
            {
                if (!cmd.Names[i].StartsWith(prefixParts[i], StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        });
    }

    /// <summary>
    /// Gets all unique top-level command categories.
    /// </summary>
    /// <returns>Distinct first-level command names.</returns>
    public IEnumerable<string> GetTopLevelCommands()
    {
        return this._commands
            .Select(c => c.Names[0])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n);
    }

    /// <summary>
    /// Gets all commands under a specific category.
    /// </summary>
    /// <param name="category">The category name (first part of command).</param>
    /// <returns>Commands in that category.</returns>
    public IEnumerable<ConsoleCommand> GetCommandsInCategory(string category)
    {
        return this._commands
            .Where(c => c.Names.Count > 0 &&
                        c.Names[0].Equals(category, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.FullName);
    }

    private int GetMatchLength(IReadOnlyList<string> commandNames, string[] tokens)
    {
        int matchLength = 0;

        for (int i = 0; i < commandNames.Count && i < tokens.Length; i++)
        {
            if (commandNames[i].Equals(tokens[i], StringComparison.OrdinalIgnoreCase))
            {
                matchLength++;
            }
            else
            {
                break;
            }
        }

        return matchLength == commandNames.Count ? matchLength : 0;
    }
}
