using System;

namespace Imago.Diagnostics;

/// <summary>
/// Shows a crash on the error output of the console. Use it when nobody watches the window, like in scripts.
/// </summary>
public sealed class ConsoleCrashPresenter : ICrashPresenter
{
    /// <inheritdoc />
    public void Present(string title, Exception exception, string? logPath)
    {
        Console.Error.WriteLine($"{title} crashed: {exception}");
        if (logPath != null)
        {
            Console.Error.WriteLine($"The log is at {logPath}");
        }
    }
}
