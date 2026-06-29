using System;
using System.Collections.Generic;
using System.IO;

namespace Imago.ScenarioRunner;

/// <summary>
/// The parsed scenario command line: any scenario names, plus the recognized options.
/// </summary>
internal sealed record ScenarioCommandLine(
    IReadOnlyList<string> Names,
    string? BackendName,
    string? ReportPath,
    bool ShowHelp,
    bool ShowList,
    bool ShowWindow,
    bool Isolated,
    bool Worker,
    bool Baseline,
    IReadOnlyList<string> UnknownOptions)
{
    /// <summary>
    /// Parses the runner's command-line arguments. Bare tokens are scenario names; an unrecognized token that
    /// starts with a dash is collected in <see cref="UnknownOptions"/>.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The parsed command line.</returns>
    public static ScenarioCommandLine Parse(string[] args)
    {
        var names = new List<string>();
        var unknown = new List<string>();
        string? backendName = null;
        string? reportPath = null;
        bool showHelp = false;
        bool showList = false;
        bool showWindow = false;
        bool isolated = false;
        bool worker = false;
        bool baseline = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--help" or "-h":
                    showHelp = true;
                    break;
                case "--list":
                    showList = true;
                    break;
                case "--show":
                    showWindow = true;
                    break;
                case "--isolated":
                    isolated = true;
                    break;
                case "--worker":
                    worker = true;
                    break;
                case "--baseline":
                    baseline = true;
                    break;
                case "--backend" when i + 1 < args.Length:
                    backendName = args[++i];
                    break;
                case "--report" when i + 1 < args.Length:
                    reportPath = args[++i];
                    break;
                default:
                    if (args[i].StartsWith('-'))
                    {
                        unknown.Add(args[i]);
                    }
                    else
                    {
                        names.Add(args[i]);
                    }

                    break;
            }
        }

        return new ScenarioCommandLine(names, backendName, reportPath, showHelp, showList, showWindow, isolated, worker, baseline, unknown);
    }

    /// <summary>
    /// Prints the usage, the options, and the scenario list.
    /// </summary>
    /// <param name="scenarios">The scenarios to list.</param>
    public static void PrintHelp(IReadOnlyList<ScenarioDescriptor> scenarios)
    {
        PrintUsage();
        Console.WriteLine();
        PrintScenarios(scenarios);
    }

    private static void PrintUsage()
    {
        string executable = Path.GetFileNameWithoutExtension(Environment.ProcessPath) ?? "scenarios";
        Console.WriteLine($"Usage: {executable} [scenario...] [options]");
        Console.WriteLine();
        Console.WriteLine("  Runs the named scenarios, or every scenario when none are named. Scenarios that share a");
        Console.WriteLine("  backend run together in one process.");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --baseline         save this run as the baseline for later runs to compare against");
        Console.WriteLine("  --isolated         run each scenario in its own process");
        Console.WriteLine("  --backend <name>   force a graphics backend");
        Console.WriteLine("  --show             open a real window instead of running hidden");
        Console.WriteLine("  --list             list the scenarios and exit");
        Console.WriteLine("  --help, -h         show this help");
    }

    /// <summary>
    /// Prints every discovered scenario, marking the ones that always run on their own.
    /// </summary>
    /// <param name="scenarios">The scenarios to list.</param>
    public static void PrintScenarios(IReadOnlyList<ScenarioDescriptor> scenarios)
    {
        Console.WriteLine("Scenarios:");
        foreach (var scenario in scenarios)
        {
            string isolated = scenario.Isolated ? "  [isolated]" : string.Empty;
            Console.WriteLine($"  {scenario.Name,-18} {scenario.Description}{isolated}");
        }
    }
}
