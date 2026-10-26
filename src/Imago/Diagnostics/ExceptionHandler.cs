using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Imago.Support;
using Microsoft.Extensions.Logging;

namespace Imago.Diagnostics;

/// <summary>
/// Reports exceptions to the log of an application and handles its crashes.
/// </summary>
/// <remarks>
/// While it is alive, <see cref="Log"/> writes to its logger, and exceptions that no code catches on other threads or
/// in lost tasks are reported too. The log is also written when the process exits before the handler is disposed.
/// </remarks>
internal sealed class ExceptionHandler : IDisposable
{
    private readonly ILogger _logger;
    private readonly ExceptionsBuilder _settings;
    private readonly string _title;
    private readonly string? _logPath;

    private bool _isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandler"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory of the application. The handler disposes it.</param>
    /// <param name="settings">The settings for reports and crashes.</param>
    /// <param name="title">The title of the application.</param>
    /// <param name="logPath">The full path of the log file, or null if no file is written.</param>
    public ExceptionHandler(ILoggerFactory loggerFactory, ExceptionsBuilder settings, string title, string? logPath)
    {
        this.LoggerFactory = loggerFactory;
        this._logger = loggerFactory.CreateLogger(string.Empty);
        this._settings = settings;
        this._title = title;
        this._logPath = logPath;

        Log.Configure(this._logger, this.Report);

        AppDomain.CurrentDomain.UnhandledException += this.AppDomain_UnhandledException;
        AppDomain.CurrentDomain.ProcessExit += this.AppDomain_ProcessExit;
        TaskScheduler.UnobservedTaskException += this.TaskScheduler_UnobservedTaskException;
    }

    /// <summary>
    /// Gets the logger factory of the application.
    /// </summary>
    public ILoggerFactory LoggerFactory { get; }

    /// <summary>
    /// Reports a handled exception, unless its type is ignored.
    /// </summary>
    /// <param name="exception">The exception to report.</param>
    public void Report(Exception exception)
    {
        exception = Unwrap(exception);

        foreach (var isIgnored in this._settings.IgnoredExceptions)
        {
            if (isIgnored(exception)) return;
        }

        this.Write(LogLevel.Error, exception);
    }

    /// <summary>
    /// Reports a crash, writes the log, shows the crash to the player, and ends the process.
    /// </summary>
    /// <param name="exception">The exception that crashed the application.</param>
    [DoesNotReturn]
    public void Crash(Exception exception)
    {
        exception = Unwrap(exception);
        this.Write(LogLevel.Critical, exception);
        this.Dispose();

        try
        {
            this._settings.CrashPresenter.Present(this._title, exception, this._logPath);
        }
        catch (Exception presenterException)
        {
            // The log is closed by now, so the console is the only place left.
            Console.Error.WriteLine($"Failed to show the crash: {presenterException}");
        }

        Environment.Exit(1);
    }

    /// <summary>
    /// Writes the remaining log lines and stops reporting.
    /// </summary>
    public void Dispose()
    {
        if (this._isDisposed) return;
        this._isDisposed = true;

        AppDomain.CurrentDomain.UnhandledException -= this.AppDomain_UnhandledException;
        AppDomain.CurrentDomain.ProcessExit -= this.AppDomain_ProcessExit;
        TaskScheduler.UnobservedTaskException -= this.TaskScheduler_UnobservedTaskException;

        Log.Reset();
        this.LoggerFactory.Dispose();
    }

    private static Exception Unwrap(Exception exception)
    {
        if (exception is AggregateException aggregate)
        {
            var innerExceptions = aggregate.Flatten().InnerExceptions;
            if (innerExceptions.Count == 1) return innerExceptions[0];
        }

        return exception;
    }

    private static void AddApplicationContext(IDictionary<string, object?> context)
    {
        context["OS"] = RuntimeInformation.OSDescription;
        context["Runtime"] = RuntimeFeature.IsDynamicCodeSupported ? "JIT" : "AOT";

        // During startup, the parts of the application that are not created yet are null.
        if (Application.Instance is not Application app) return;
        context["Backend"] = app.Renderer.BackendType;
        context["Gpu"] = app.Renderer.GraphicsDevice.DeviceName;
        context["Uptime"] = TimeSpan.FromSeconds(app.Ticker.ElapsedTime);
        context["Screen"] = app.Stage.Screen?.GetType().Name;
    }

    private static void AddContext(Dictionary<string, object?> context, Action<IDictionary<string, object?>> addContext)
    {
        // A broken context must never stop the report itself.
        try
        {
            addContext(context);
        }
        catch (Exception e)
        {
            context["ContextError"] = e.Message;
        }
    }

    private void Write(LogLevel level, Exception exception)
    {
        var context = new Dictionary<string, object?>();
        AddContext(context, AddApplicationContext);

        foreach (var addContext in this._settings.Contexts)
        {
            AddContext(context, addContext);
        }

        foreach (DictionaryEntry entry in exception.Data)
        {
            context[entry.Key.ToString() ?? string.Empty] = entry.Value;
        }

        this._logger.Log(level, exception, "{ExceptionMessage} {Context}", exception.Message, context);
    }

    private void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            this.Write(LogLevel.Critical, Unwrap(exception));
        }

        // The process ends right after this event, so this is the last chance to write the log.
        this.Dispose();
    }

    private void AppDomain_ProcessExit(object? sender, EventArgs e)
    {
        this.Dispose();
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        this.Report(e.Exception);
        e.SetObserved();
    }
}
