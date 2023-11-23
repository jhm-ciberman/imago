using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LifeSim.Support;

public static class Benchmark
{
    private static readonly Action<object> _defaultLogger = Console.WriteLine;
    private static Action<object> _loggerFunction = _defaultLogger;

    private static readonly Dictionary<string, Stopwatch> _parts = new Dictionary<string, Stopwatch>();

    public static void SetLogger(Action<object> logger)
    {
        _loggerFunction = logger ?? _defaultLogger;
    }

    public static T Run<T>(string taskName, Func<T> callback)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var value = callback();

        sw.Stop();

        _loggerFunction($"\"{taskName}\" took {sw.ElapsedMilliseconds} milliseconds");

        return value;
    }

    public static void Run(string taskName, Action callback)
    {
        Stopwatch sw = Stopwatch.StartNew();

        callback();

        sw.Stop();

        _loggerFunction($"\"{taskName}\" took {sw.ElapsedMilliseconds} milliseconds");
    }


    public static void RunTicks(string taskName, Action callback)
    {
        Stopwatch sw = Stopwatch.StartNew();

        callback();

        sw.Stop();

        _loggerFunction("\"" + taskName + "\" took " + sw.ElapsedTicks + " ticks");
    }

    private static Stopwatch GetStopWatch(string taskName)
    {
        if (!_parts.TryGetValue(taskName, out Stopwatch? sw))
        {
            sw = new Stopwatch();
            _parts.Add(taskName, sw);
        }
        return sw;
    }

    public static void RunPart(string taskName, Action callback)
    {
        var sw = Benchmark.GetStopWatch(taskName);
        sw.Start();
        callback();
        sw.Stop();

        Benchmark._loggerFunction("\"" + taskName + "\" took " + sw.ElapsedMilliseconds + " milliseconds");
    }

    public static void Report(string taskName)
    {
        var sw = Benchmark.GetStopWatch(taskName);
        Benchmark._loggerFunction("\"" + taskName + "\" took " + sw.ElapsedMilliseconds + " milliseconds");
        Benchmark._parts.Remove(taskName);
    }

}
