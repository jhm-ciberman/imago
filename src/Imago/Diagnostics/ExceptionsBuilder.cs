using System;
using System.Collections.Generic;

namespace Imago.Diagnostics;

/// <summary>
/// Collects how an application reports exceptions and shows crashes, before it is created.
/// </summary>
public sealed class ExceptionsBuilder
{
    private readonly List<Func<Exception, bool>> _ignoredExceptions = [];
    private readonly List<Action<IDictionary<string, object?>>> _contexts = [];

    internal IReadOnlyList<Func<Exception, bool>> IgnoredExceptions => this._ignoredExceptions;

    internal IReadOnlyList<Action<IDictionary<string, object?>>> Contexts => this._contexts;

    internal ICrashPresenter CrashPresenter { get; private set; } = new MessageBoxCrashPresenter();

    /// <summary>
    /// Stops reporting handled exceptions of the given type.
    /// </summary>
    /// <remarks>
    /// A crash is always reported, whatever its type.
    /// </remarks>
    /// <typeparam name="TException">The type of the exceptions to ignore, including derived types.</typeparam>
    /// <returns>This builder.</returns>
    public ExceptionsBuilder DontReport<TException>() where TException : Exception
    {
        this._ignoredExceptions.Add(exception => exception is TException);
        return this;
    }

    /// <summary>
    /// Adds values to every report, such as the current screen.
    /// </summary>
    /// <param name="context">The callback that adds the values to the context of a report. It runs each time an exception is reported.</param>
    /// <returns>This builder.</returns>
    public ExceptionsBuilder Context(Action<IDictionary<string, object?>> context)
    {
        this._contexts.Add(context);
        return this;
    }

    /// <summary>
    /// Sets how a crash is shown to the player. Defaults to <see cref="MessageBoxCrashPresenter"/>.
    /// </summary>
    /// <typeparam name="TPresenter">The type of the presenter.</typeparam>
    /// <returns>This builder.</returns>
    public ExceptionsBuilder ShowCrashWith<TPresenter>() where TPresenter : ICrashPresenter, new()
    {
        this.CrashPresenter = new TPresenter();
        return this;
    }

    /// <summary>
    /// Shows a crash in a native message box. This is the default.
    /// </summary>
    /// <returns>This builder.</returns>
    public ExceptionsBuilder ShowCrashInMessageBox()
    {
        return this.ShowCrashWith<MessageBoxCrashPresenter>();
    }

    /// <summary>
    /// Shows a crash on the error output of the console.
    /// </summary>
    /// <returns>This builder.</returns>
    public ExceptionsBuilder ShowCrashInConsole()
    {
        return this.ShowCrashWith<ConsoleCrashPresenter>();
    }
}
