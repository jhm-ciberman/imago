using System;
namespace LifeSim.Support;

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

/// <summary>
/// This class is used to create commands that can be executed.
/// </summary>
public class RelayCommand : ICommand, IRelayCommand
{
    /// <summary>
    /// Raised whener the CanExecute of the command changes.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    private readonly Action _command;
    private readonly Func<bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="canExecute">The function that determines whether the command can execute.</param>
    public RelayCommand(Action command, Func<bool>? canExecute = null)
    {
        this._command = command;
        this._canExecute = canExecute;
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    public void Execute(object? parameter)
    {
        if (this.CanExecute(parameter))
        {
            this._command.Invoke();
        }
    }

    /// <summary>
    /// Determines whether this command can execute.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>True if the command can execute, otherwise false.</returns>
    public bool CanExecute(object? parameter)
    {
        if (this._canExecute == null)
        {
            return true;
        }

        return this._canExecute.Invoke();
    }

    /// <summary>
    /// Raises the CanExecuteChanged event.
    /// </summary>
    public void OnCanExecuteChanged()
    {
        this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>
/// This class is used to create commands that can be executed and receive a parameter of type T.
/// </summary>
/// <typeparam name="T">The type of the parameter.</typeparam>
public class RelayCommand<T> : ICommand, IRelayCommand, IRelayCommand<T>
{
    /// <summary>
    /// Raised whener the CanExecute of the command changes.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    private readonly Action<T> _command;
    private readonly Func<T, bool>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="canExecute">The function that determines whether the command can execute.</param>
    public RelayCommand(Action<T> command, Func<T, bool>? canExecute = null)
    {
        this._command = command;
        this._canExecute = canExecute;
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    public void Execute(object? parameter)
    {
        if (parameter is not T t)
        {
            throw new ArgumentException($"The parameter must be of type {typeof(T).Name}.");
        }

        this.Execute(t);
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    public void Execute(T parameter)
    {
        if (this.CanExecute(parameter))
        {
            this._command.Invoke(parameter);
        }
    }

    /// <summary>
    /// Determines whether this command can execute.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>True if the command can execute, otherwise false.</returns>
    public bool CanExecute(object? parameter)
    {
        if (this._canExecute == null)
        {
            return true;
        }

        if (parameter is not T t)
        {
            throw new ArgumentException($"The parameter must be of type {typeof(T).Name}.");
        }

        return this._canExecute.Invoke(t);
    }

    /// <summary>
    /// Determines whether this command can execute.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>True if the command can execute, otherwise false.</returns>
    public bool CanExecute(T parameter)
    {
        if (this._canExecute == null)
        {
            return true;
        }

        return this._canExecute.Invoke(parameter);
    }

    /// <summary>
    /// Raises the CanExecuteChanged event.
    /// </summary>
    public void OnCanExecuteChanged()
    {
        this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

