namespace Imago.DevConsole;

/// <summary>
/// Defines an argument for a console command.
/// </summary>
public class CommandArgument
{
    /// <summary>
    /// Gets the name of the argument.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description of the argument.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets a value indicating whether this argument is required.
    /// </summary>
    public bool IsRequired { get; }

    /// <summary>
    /// Gets the default value for optional arguments.
    /// </summary>
    public string? DefaultValue { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandArgument"/> class.
    /// </summary>
    /// <param name="name">The argument name.</param>
    /// <param name="description">The argument description.</param>
    /// <param name="isRequired">Whether the argument is required.</param>
    /// <param name="defaultValue">The default value for optional arguments.</param>
    public CommandArgument(string name, string description, bool isRequired = true, string? defaultValue = null)
    {
        this.Name = name;
        this.Description = description;
        this.IsRequired = isRequired;
        this.DefaultValue = defaultValue;
    }
}
