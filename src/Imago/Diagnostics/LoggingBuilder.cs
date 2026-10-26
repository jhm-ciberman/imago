using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Imago.Diagnostics;

/// <summary>
/// Collects the logging settings of an application before it is created.
/// </summary>
public sealed class LoggingBuilder
{
    private const string OutputTemplate = "[{Timestamp:HH:mm:ss.fff}] {Level:u4} {Message:lj}{NewLine}{Exception}";

    private readonly List<Action<LoggerConfiguration>> _serilogConfigurations = [];

    /// <summary>
    /// Gets or sets the lowest level that is written. Defaults to debug in debug builds, and to information otherwise.
    /// </summary>
    public LogLevel MinimumLevel { get; set; } = Application.IsDebug ? LogLevel.Debug : LogLevel.Information;

    /// <summary>
    /// Gets or sets a value indicating whether log lines are written to the console. Defaults to true.
    /// </summary>
    public bool Console { get; set; } = true;

    /// <summary>
    /// Gets or sets the file that log lines are written to, or null to write no file. Defaults to <c>logs/game.log</c>.
    /// </summary>
    /// <remarks>
    /// Each run starts a new file. The files of earlier runs get a number, like <c>game.1.log</c> for the previous run.
    /// </remarks>
    public string? FilePath { get; set; } = "logs/game.log";

    /// <summary>
    /// Gets or sets how many log files are kept, the current one included. Defaults to 5.
    /// </summary>
    public int KeepFiles { get; set; } = 5;

    /// <summary>
    /// Adds settings to the Serilog logger, such as more sinks.
    /// </summary>
    /// <param name="configure">The callback that changes the Serilog configuration.</param>
    /// <returns>This builder.</returns>
    public LoggingBuilder Serilog(Action<LoggerConfiguration> configure)
    {
        this._serilogConfigurations.Add(configure);
        return this;
    }

    internal ILoggerFactory Build()
    {
        Guard.IsGreaterThanOrEqualTo(this.KeepFiles, 1);

        var configuration = new LoggerConfiguration().MinimumLevel.Is(ToSerilogLevel(this.MinimumLevel));

        if (this.Console)
        {
            configuration.WriteTo.Async(sink => sink.Console(outputTemplate: OutputTemplate));
        }

        if (this.FilePath is string filePath)
        {
            RotateFiles(filePath, this.KeepFiles);
            configuration.WriteTo.Async(sink => sink.File(filePath, outputTemplate: OutputTemplate));
        }

        foreach (var configure in this._serilogConfigurations)
        {
            configure(configuration);
        }

        return new SerilogLoggerFactory(configuration.CreateLogger(), dispose: true);
    }

    private static LogEventLevel ToSerilogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,

            // Serilog has no level above Fatal, so one past it lets nothing through.
            _ => LogEventLevel.Fatal + 1,
        };
    }

    private static void RotateFiles(string filePath, int keepFiles)
    {
        try
        {
            File.Delete(GetRotatedPath(filePath, keepFiles - 1));

            for (int i = keepFiles - 2; i >= 0; i--)
            {
                string source = GetRotatedPath(filePath, i);
                if (File.Exists(source))
                {
                    File.Move(source, GetRotatedPath(filePath, i + 1));
                }
            }
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Another running copy of the application can hold the files open. The new lines then go after the old ones.
        }
    }

    private static string GetRotatedPath(string filePath, int index)
    {
        if (index == 0) return filePath;

        string directory = Path.GetDirectoryName(filePath) ?? string.Empty;
        string name = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        return Path.Combine(directory, $"{name}.{index}{extension}");
    }
}
