namespace LifeSim.Support.ComponentModel;

/// <summary>
/// Represents a command that can be executed.
/// </summary>
public interface IRelayCommand : ICommand
{
    /// <summary>
    /// Raises the CanExecuteChanged event.
    /// </summary>
    void OnCanExecuteChanged();
}

/// <summary>
/// Represents a command that can be executed and receives a parameter of type T.
/// </summary>
/// <typeparam name="T">The type of the command parameter.</typeparam>
public interface IRelayCommand<T> : IRelayCommand, ICommand
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    void Execute(T parameter);

    /// <summary>
    /// Determines whether this command can execute.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>True if the command can execute, otherwise false.</returns>
    bool CanExecute(T parameter);
}

