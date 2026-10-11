using System;
using System.Collections.Generic;

namespace Imago.DevConsole;

/// <summary>
/// Fluent builder for defining inline console commands.
/// </summary>
public class CommandBuilder
{
    private readonly CommandRegistry _registry;
    private readonly string[] _names;
    private readonly List<CommandArgument> _arguments = [];
    private string _description = string.Empty;
    private bool _isHidden;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandBuilder"/> class.
    /// </summary>
    /// <param name="registry">The registry to add the command to.</param>
    /// <param name="names">The command name segments.</param>
    internal CommandBuilder(CommandRegistry registry, string[] names)
    {
        this._registry = registry;
        this._names = names;
    }

    /// <summary>
    /// Sets the command description.
    /// </summary>
    /// <param name="description">The description text.</param>
    /// <returns>This builder for chaining.</returns>
    public CommandBuilder WithDescription(string description)
    {
        this._description = description;
        return this;
    }

    /// <summary>
    /// Marks the command as hidden from help listings.
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    public CommandBuilder Hidden()
    {
        this._isHidden = true;
        return this;
    }

    /// <summary>
    /// Adds a required argument.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="description">The argument description.</param>
    /// <returns>This builder for chaining.</returns>
    public CommandBuilder WithArgument(string name, string description)
    {
        this._arguments.Add(new CommandArgument(name, description, isRequired: true));
        return this;
    }

    /// <summary>
    /// Adds an optional argument.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="description">The argument description.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>This builder for chaining.</returns>
    public CommandBuilder WithOptionalArgument(string name, string description, string? defaultValue = null)
    {
        this._arguments.Add(new CommandArgument(name, description, isRequired: false, defaultValue));
        return this;
    }

    /// <summary>
    /// Completes the command definition with a handler action.
    /// </summary>
    /// <param name="handler">The action to execute when the command is invoked.</param>
    public void Handle(Action<CommandContext> handler)
    {
        var command = new InlineCommand(this._names, this._description, this._arguments, handler, this._isHidden);
        this._registry.Register(command);
    }
}

/// <summary>
/// A console command defined inline via the fluent builder.
/// </summary>
internal sealed class InlineCommand : ConsoleCommand
{
    private readonly Action<CommandContext> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="InlineCommand"/> class.
    /// </summary>
    internal InlineCommand(
        string[] names,
        string description,
        List<CommandArgument> arguments,
        Action<CommandContext> handler,
        bool isHidden = false
    )
        : base(names, description, arguments, isHidden)
    {
        this._handler = handler;
    }

    /// <inheritdoc />
    protected override void Configure()
    {
    }

    /// <inheritdoc />
    public override void Handle(CommandContext context)
    {
        this._handler(context);
    }
}
