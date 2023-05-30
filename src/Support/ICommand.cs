using System;

namespace Support;

/// <summary>
/// Interface for commands.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Event raised whener the CanExecute of the command changes.
    /// </summary>
    event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    void Execute(object? parameter);

    /// <summary>
    /// Determines whether this command can execute.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>True if the command can execute, otherwise false.</returns>
    bool CanExecute(object? parameter);
}
