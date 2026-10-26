using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Imago.Support;

/// <summary>
/// Gives access to the logger of the application from anywhere, and reports exceptions.
/// </summary>
/// <remarks>
/// Until <see cref="Configure"/> is called, log lines are dropped.
/// </remarks>
public static class Log
{
    private static Action<Exception> _reporter = LogError;

    /// <summary>
    /// Gets the logger of the application.
    /// </summary>
    public static ILogger Default { get; private set; } = NullLogger.Instance;

    /// <summary>
    /// Sets the logger of the application and how exceptions are reported.
    /// </summary>
    /// <param name="logger">The logger of the application.</param>
    /// <param name="reporter">The action that reports exceptions, or null to log them as errors.</param>
    public static void Configure(ILogger logger, Action<Exception>? reporter = null)
    {
        Default = logger;
        _reporter = reporter ?? LogError;
    }

    /// <summary>
    /// Drops log lines again, as before <see cref="Configure"/> was called.
    /// </summary>
    public static void Reset()
    {
        Default = NullLogger.Instance;
        _reporter = LogError;
    }

    /// <summary>
    /// Reports an exception that was handled but that someone should know about.
    /// </summary>
    /// <param name="exception">The exception to report.</param>
    public static void Report(Exception exception)
    {
        _reporter(exception);
    }

    private static void LogError(Exception exception)
    {
        Default.LogError(exception, "{ExceptionMessage}", exception.Message);
    }
}
