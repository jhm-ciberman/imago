using System.Collections.Generic;

namespace Imago.DevConsole;

/// <summary>
/// Base class for all developer console commands.
/// </summary>
public abstract class ConsoleCommand
{
    private readonly List<string> _names = [];
    private readonly List<CommandArgument> _arguments = [];
    private string _description = string.Empty;
    private readonly bool _isHidden;

    /// <summary>
    /// Gets the command path (e.g., ["time", "speed"] for "time speed").
    /// </summary>
    public IReadOnlyList<string> Names => this._names;

    /// <summary>
    /// Gets the command description.
    /// </summary>
    public string Description => this._description;

    /// <summary>
    /// Gets the command arguments.
    /// </summary>
    public IReadOnlyList<CommandArgument> Arguments => this._arguments;

    /// <summary>
    /// Gets a value indicating whether this command is hidden from help listings.
    /// </summary>
    public bool IsHidden => this._isHidden;

    /// <summary>
    /// Gets the full command name as a string (e.g., "time speed").
    /// </summary>
    public string FullName => string.Join(" ", this._names);

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommand"/> class.
    /// </summary>
    protected ConsoleCommand()
    {
        this.Configure();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleCommand"/> class with direct configuration.
    /// Used internally for inline command definitions.
    /// </summary>
    internal ConsoleCommand(string[] names, string description, IEnumerable<CommandArgument> arguments, bool isHidden = false)
    {
        this._names.AddRange(names);
        this._description = description;
        this._arguments.AddRange(arguments);
        this._isHidden = isHidden;
    }

    /// <summary>
    /// Configures the command name, description, and arguments.
    /// </summary>
    protected abstract void Configure();

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context with parsed arguments.</param>
    public abstract void Handle(CommandContext context);

    /// <summary>
    /// Sets the command name path.
    /// </summary>
    /// <param name="names">The command name segments (e.g., "time", "speed").</param>
    protected void SetName(params string[] names)
    {
        this._names.Clear();
        this._names.AddRange(names);
    }

    /// <summary>
    /// Sets the command description.
    /// </summary>
    /// <param name="description">The description text.</param>
    protected void SetDescription(string description)
    {
        this._description = description;
    }

    /// <summary>
    /// Adds a required argument to the command.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="description">The argument description.</param>
    protected void AddArgument(string name, string description)
    {
        this._arguments.Add(new CommandArgument(name, description, isRequired: true));
    }

    /// <summary>
    /// Adds an optional argument to the command.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="description">The argument description.</param>
    /// <param name="defaultValue">The default value.</param>
    protected void AddOptionalArgument(string name, string description, string? defaultValue = null)
    {
        this._arguments.Add(new CommandArgument(name, description, isRequired: false, defaultValue));
    }

    /// <summary>
    /// Gets the usage string for this command.
    /// </summary>
    /// <returns>A string showing how to use the command.</returns>
    public string GetUsage()
    {
        var usage = this.FullName;

        foreach (var arg in this._arguments)
        {
            usage += arg.IsRequired ? $" <{arg.Name}>" : $" [{arg.Name}]";
        }

        return usage;
    }

    /// <summary>
    /// Gets the help text for this command.
    /// </summary>
    /// <returns>Full help text including usage and argument descriptions.</returns>
    public string GetHelp()
    {
        var help = $"{this.GetUsage()}\n  {this.Description}";

        if (this._arguments.Count > 0)
        {
            help += "\n\nArguments:";
            foreach (var arg in this._arguments)
            {
                var required = arg.IsRequired ? "required" : $"optional, default: {arg.DefaultValue ?? "none"}";
                help += $"\n  {arg.Name} - {arg.Description} ({required})";
            }
        }

        return help;
    }
}
