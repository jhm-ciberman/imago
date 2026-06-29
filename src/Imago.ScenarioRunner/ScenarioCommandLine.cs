using System;
using System.Collections.Generic;
using System.IO;

namespace Imago.ScenarioRunner;

/// <summary>
/// The parsed scenario command line: any scenario names, plus the recognized options.
/// </summary>
internal sealed record ScenarioCommandLine(
    IReadOnlyList<string> Names,
    string? Filter,
    string? BackendName,
    bool ShowList,
    bool ShowWindow,
    bool Isolated,
    bool Worker)
{
    /// <summary>
    /// Parses the runner's command-line arguments. Unknown leading tokens are taken as scenario names.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The parsed command line.</returns>
    public static ScenarioCommandLine Parse(string[] args)
    {
        var names = new List<string>();
        string? filter = null;
        string? backendName = null;
        bool showList = false;
        bool showWindow = false;
        bool isolated = false;
        bool worker = false;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
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
                case "--filter" when i + 1 < args.Length:
                    filter = args[++i];
                    break;
                case "--backend" when i + 1 < args.Length:
                    backendName = args[++i];
                    break;
                default:
                    if (!args[i].StartsWith('-'))
                    {
                        names.Add(args[i]);
                    }

                    break;
            }
        }

        return new ScenarioCommandLine(names, filter, backendName, showList, showWindow, isolated, worker);
    }

    /// <summary>
    /// Prints the usage line and every discovered scenario, marking the ones that always run on their own.
    /// </summary>
    /// <param name="scenarios">The scenarios to list.</param>
    public static void PrintList(IReadOnlyList<ScenarioDescriptor> scenarios)
    {
        string executable = Path.GetFileNameWithoutExtension(Environment.ProcessPath) ?? "scenarios";
        Console.WriteLine($"Usage: {executable} [scenario...] [--filter <text>] [--backend <name>] [--isolated] [--show] [--list]");
        Console.WriteLine("  With no scenario name, every scenario runs. Scenarios sharing a backend share a process;");
        Console.WriteLine("  pass --isolated to run each in its own process instead.");
        Console.WriteLine();
        Console.WriteLine("Scenarios:");
        foreach (var scenario in scenarios)
        {
            string isolated = scenario.Isolated ? "  [isolated]" : string.Empty;
            Console.WriteLine($"  {scenario.Name,-18} {scenario.Description}{isolated}");
        }
    }
}
