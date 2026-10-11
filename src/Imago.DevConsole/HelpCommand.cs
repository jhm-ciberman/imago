using System.Linq;

namespace Imago.DevConsole;

/// <summary>
/// Built-in command that displays help information.
/// </summary>
internal sealed class HelpCommand : ConsoleCommand
{
    /// <inheritdoc />
    protected override void Configure()
    {
        this.SetName("help");
        this.SetDescription("Shows available commands or help for a specific command");
        this.AddOptionalArgument("command", "The command to get help for");
    }

    /// <inheritdoc />
    public override void Handle(CommandContext context)
    {
        var console = context.Console;

        if (context.TryGetArgument("command", out var commandName) && !string.IsNullOrEmpty(commandName))
        {
            this.ShowCommandHelp(console, commandName);
        }
        else
        {
            this.ShowAllCommands(console);
        }
    }

    private void ShowAllCommands(DeveloperConsole console)
    {
        console.WriteLine("Available commands:");
        console.WriteLine("");

        var categories = console.Registry.GetTopLevelCommands().ToList();

        foreach (var category in categories)
        {
            var commands = console.Registry.GetCommandsInCategory(category)
                .Where(c => !c.IsHidden)
                .ToList();

            if (commands.Count == 0)
            {
                continue;
            }

            if (commands.Count == 1 && commands[0].Names.Count == 1)
            {
                console.WriteLine($"  {commands[0].GetUsage(),-30} {commands[0].Description}");
            }
            else
            {
                console.WriteLine($"  {category}");
                foreach (var cmd in commands)
                {
                    console.WriteLine($"    {cmd.GetUsage(),-28} {cmd.Description}");
                }
            }
        }

        console.WriteLine("");
        console.WriteLine("Type 'help <command>' for detailed help.");
    }

    private void ShowCommandHelp(DeveloperConsole console, string commandName)
    {
        var commands = console.Registry.GetCommandsWithPrefix(commandName).ToList();

        if (commands.Count == 0)
        {
            console.WriteError($"Unknown command: {commandName}");
            return;
        }

        if (commands.Count == 1)
        {
            console.WriteLine(commands[0].GetHelp());
        }
        else
        {
            console.WriteLine($"Commands matching '{commandName}':");
            foreach (var cmd in commands)
            {
                console.WriteLine($"  {cmd.GetUsage(),-30} {cmd.Description}");
            }
        }
    }
}
