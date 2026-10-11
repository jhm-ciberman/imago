using System;
using System.Collections.Generic;

namespace Imago.DevConsole;

/// <summary>
/// Specifies the kind of a console output line.
/// </summary>
public enum ConsoleLineKind
{
    /// <summary>
    /// A regular output line.
    /// </summary>
    Output,

    /// <summary>
    /// A line representing a user-entered command.
    /// </summary>
    Command,

    /// <summary>
    /// An error message line.
    /// </summary>
    Error,
}

/// <summary>
/// Represents a line of output in the developer console.
/// </summary>
/// <param name="Text">The text content of the line.</param>
/// <param name="Kind">The kind of console line.</param>
public readonly record struct ConsoleLine(string Text, ConsoleLineKind Kind = ConsoleLineKind.Output);

/// <summary>
/// The model for the developer console, managing output and command execution.
/// </summary>
public class DeveloperConsole
{
    private readonly List<ConsoleLine> _lines = [];
    private readonly List<string> _history = [];
    private readonly CommandRegistry _registry = new();

    /// <summary>
    /// Occurs when the console output changes.
    /// </summary>
    public event EventHandler? OutputChanged;

    /// <summary>
    /// Occurs when the console requests to be closed.
    /// </summary>
    public event EventHandler? ExitRequested;

    /// <summary>
    /// Gets the output lines of the console.
    /// </summary>
    public IReadOnlyList<ConsoleLine> Lines => this._lines;

    /// <summary>
    /// Gets the command history.
    /// </summary>
    public IReadOnlyList<string> History => this._history;

    /// <summary>
    /// Gets the command registry.
    /// </summary>
    public CommandRegistry Registry => this._registry;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeveloperConsole"/> class.
    /// </summary>
    public DeveloperConsole()
    {
        this.RegisterBuiltInCommands();
        this.WriteLine("Developer Console.");
        this.WriteLine("Type 'help' for available commands.");
        this.WriteLine("Type 'exit', 'q' or press <Esc> to close the console.");
    }

    /// <summary>
    /// Writes a line of text to the console output.
    /// </summary>
    /// <param name="text">The text to write.</param>
    public void WriteLine(string text)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            this._lines.Add(new ConsoleLine(line));
        }

        this.OutputChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Writes an error line to the console output.
    /// </summary>
    /// <param name="text">The error text to write.</param>
    public void WriteError(string text)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            this._lines.Add(new ConsoleLine(line, ConsoleLineKind.Error));
        }

        this.OutputChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Clears all output from the console.
    /// </summary>
    public void Clear()
    {
        this._lines.Clear();
        this.OutputChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Requests the console to close.
    /// </summary>
    public void RequestExit()
    {
        this.ExitRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Submits a command to the console for execution.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    public void SubmitCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        this._history.Add(command);
        this._lines.Add(new ConsoleLine(command, ConsoleLineKind.Command));
        this.ExecuteCommand(command);
        this.OutputChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RegisterBuiltInCommands()
    {
        this._registry.Register<HelpCommand>();

        this._registry.Register("clear")
            .WithDescription("Clears the console output")
            .Handle(ctx => ctx.Console.Clear());

        this._registry.Register("echo")
            .WithDescription("Prints a message to the console")
            .WithArgument("message", "The message to print")
            .Handle(ctx => ctx.Console.WriteLine(ctx.GetString("message")));

        this._registry.Register("exit")
            .WithDescription("Closes the console")
            .Handle(ctx => ctx.Console.RequestExit());

        this._registry.Register("q").Hidden().Handle(ctx => ctx.Console.RequestExit());
        this._registry.Register("quit").Hidden().Handle(ctx => ctx.Console.RequestExit());
        this._registry.Register(":q").Hidden().Handle(ctx => ctx.Console.WriteLine("This is not Vim! lol"));
        this._registry.Register(":wq").Hidden().Handle(ctx => ctx.Console.WriteLine("This is not Vim! lol"));
    }

    private void ExecuteCommand(string input)
    {
        var tokens = this.Tokenize(input);

        if (tokens.Length == 0)
        {
            return;
        }

        if (!this._registry.TryFindCommand(tokens, out var command, out var argumentTokens))
        {
            this._lines.Add(new ConsoleLine(
                $"Unknown command: {tokens[0]}. Type 'help' for available commands.",
                ConsoleLineKind.Error
            ));
            return;
        }

        var arguments = this.ParseArguments(command!, argumentTokens);
        if (arguments == null)
        {
            return;
        }

        var context = new CommandContext(this, arguments);

        try
        {
            command!.Handle(context);
        }
        catch (Exception ex)
        {
            this._lines.Add(new ConsoleLine($"Error: {ex.Message}", ConsoleLineKind.Error));
        }
    }

    private string[] Tokenize(string input)
    {
        var tokens = new List<string>();
        var current = string.Empty;
        var inQuotes = false;

        foreach (var c in input)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (current.Length > 0)
                {
                    tokens.Add(current);
                    current = string.Empty;
                }
            }
            else
            {
                current += c;
            }
        }

        if (current.Length > 0)
        {
            tokens.Add(current);
        }

        return [.. tokens];
    }

    private Dictionary<string, string>? ParseArguments(ConsoleCommand command, string[] tokens)
    {
        var arguments = new Dictionary<string, string>();
        var argDefs = command.Arguments;

        for (int i = 0; i < argDefs.Count; i++)
        {
            var argDef = argDefs[i];

            if (i < tokens.Length)
            {
                arguments[argDef.Name] = tokens[i];
            }
            else if (argDef.IsRequired)
            {
                this._lines.Add(new ConsoleLine(
                    $"Missing required argument: {argDef.Name}",
                    ConsoleLineKind.Error
                ));
                this._lines.Add(new ConsoleLine(
                    $"Usage: {command.GetUsage()}",
                    ConsoleLineKind.Error
                ));
                return null;
            }
            else if (argDef.DefaultValue != null)
            {
                arguments[argDef.Name] = argDef.DefaultValue;
            }
        }

        return arguments;
    }
}
