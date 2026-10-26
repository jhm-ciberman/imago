using System;
using Silk.NET.SDL;

namespace Imago.Diagnostics;

/// <summary>
/// Shows a crash in a native message box. It works even when the renderer is broken.
/// </summary>
public sealed class MessageBoxCrashPresenter : ICrashPresenter
{
    /// <inheritdoc />
    public unsafe void Present(string title, Exception exception, string? logPath)
    {
        string message = $"{title} crashed.\n\n{exception.Message}";
        if (logPath != null)
        {
            message += $"\n\nThe log is at {logPath}";
        }

        Sdl.GetApi().ShowSimpleMessageBox((uint)MessageBoxFlags.Error, title, message, null);
    }
}
