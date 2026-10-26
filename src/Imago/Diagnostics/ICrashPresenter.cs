using System;

namespace Imago.Diagnostics;

/// <summary>
/// Shows the player that the application crashed.
/// </summary>
public interface ICrashPresenter
{
    /// <summary>
    /// Shows that the application crashed. Called once, after the crash was reported and the log was written.
    /// </summary>
    /// <param name="title">The title of the application.</param>
    /// <param name="exception">The exception that crashed the application.</param>
    /// <param name="logPath">The full path of the log file, or null if no file is written.</param>
    public void Present(string title, Exception exception, string? logPath);
}
