using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LifeSim;

public static class Benchmark
{
    private static readonly Action<object> _defaultLogger = System.Console.WriteLine;
    private static Action<object> _loggerFunction = Benchmark._defaultLogger;

    private static readonly Dictionary<string, Stopwatch> _parts = new Dictionary<string, Stopwatch>();

    public static void SetLogger(Action<object> logger)
    {
        Benchmark._loggerFunction = logger ?? Benchmark._defaultLogger;
    }

    public static T Run<T>(string taskName, Func<T> callback)
    {
        Stopwatch sw = Stopwatch.StartNew();

        var value = callback();

        sw.Stop();

        Benchmark._loggerFunction("\"" + taskName + "\" took " + sw.ElapsedMilliseconds + " milliseconds");

        return value;
    }

    public static void Run(string taskName, Action callback)
    {
        Stopwatch sw = Stopwatch.StartNew();

        callback();

        sw.Stop();

        Benchmark._loggerFunction("\"" + taskName + "\" took " + sw.ElapsedMilliseconds + " milliseconds");
    }


    public static void RunTicks(string taskName, Action callback)
    {
        Stopwatch sw = Stopwatch.StartNew();

        callback();

        sw.Stop();

        Benchmark._loggerFunction("\"" + taskName + "\" took " + sw.ElapsedTicks + " ticks");
    }

    private static Stopwatch _GetStopWatch(string taskName)
    {
        if (!Benchmark._parts.TryGetValue(taskName, out Stopwatch? sw))
        {
            sw = new Stopwatch();
            Benchmark._parts.Add(taskName, sw);
        }
        return sw;
    }

    public static void RunPart(string taskName, Action callback)
    {
        var sw = Benchmark._GetStopWatch(taskName);
        sw.Start();
        callback();
        sw.Stop();

        Benchmark._loggerFunction("\"" + taskName + "\" took " + sw.ElapsedMilliseconds + " milliseconds");
    }

    public static void Report(string taskName)
    {
        var sw = Benchmark._GetStopWatch(taskName);
        Benchmark._loggerFunction("\"" + taskName + "\" took " + sw.ElapsedMilliseconds + " milliseconds");
        Benchmark._parts.Remove(taskName);
    }

}