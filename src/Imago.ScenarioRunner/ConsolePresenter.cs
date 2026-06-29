using System;
using System.Collections.Generic;
using System.IO;
using Imago.Support.Numerics;

namespace Imago.ScenarioRunner;

/// <summary>
/// Renders scenario progress and results. Callers raise semantic events ("a scenario passed"); this class
/// decides how they look.
/// </summary>
/// <remarks>
/// Writes to the stream it is given rather than <see cref="Console"/> directly, so the runner can redirect the
/// game's own console output elsewhere and keep this output clean. Color is dropped when the output is redirected.
/// </remarks>
internal sealed class ConsolePresenter
{
    private const int LineWidth = 56;

    private readonly TextWriter _out;
    private readonly bool _color = !Console.IsOutputRedirected;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsolePresenter"/> class.
    /// </summary>
    /// <param name="output">The stream to render to, typically the real standard output.</param>
    public ConsolePresenter(TextWriter output)
    {
        this._out = output;
    }

    /// <summary>
    /// Announces the start of a run.
    /// </summary>
    /// <param name="scenarios">The number of scenarios about to run.</param>
    /// <param name="processes">The number of processes they run across.</param>
    public void RunStarted(int scenarios, int processes)
    {
        this._out.WriteLine();
        string label = $"  {scenarios} scenario{Plural(scenarios)}";
        if (processes > 1)
        {
            label += $" · {processes} processes";
        }

        this.Write(label, ConsoleColor.DarkGray);
        this._out.WriteLine();
        this._out.WriteLine();
    }

    /// <summary>
    /// Reports a progress message logged from inside a scenario.
    /// </summary>
    /// <param name="message">The message to show.</param>
    public void Progress(string message)
    {
        this.Write($"  · {message}", ConsoleColor.DarkGray);
        this._out.WriteLine();
    }

    /// <summary>
    /// Reports that a scenario finished successfully.
    /// </summary>
    /// <param name="name">The scenario name.</param>
    /// <param name="shots">The labels of the shots it captured, in order.</param>
    /// <param name="seconds">How long the scenario took.</param>
    public void ScenarioPassed(string name, IReadOnlyList<string> shots, double seconds)
    {
        string count = $"{shots.Count} shot{Plural(shots.Count)}";
        this.ResultLine('✓', ConsoleColor.Green, name, $"{count} · {seconds:0.0}s", ConsoleColor.DarkGray);

        if (shots.Count > 1)
        {
            this.Write($"      {string.Join(" · ", shots)}", ConsoleColor.DarkGray);
            this._out.WriteLine();
        }
    }

    /// <summary>
    /// Reports that a scenario failed.
    /// </summary>
    /// <param name="name">The scenario name.</param>
    /// <param name="seconds">How long the scenario ran before failing.</param>
    /// <param name="error">The error that ended it.</param>
    public void ScenarioFailed(string name, double seconds, Exception error)
    {
        this.ResultLine('✗', ConsoleColor.Red, name, "FAILED", ConsoleColor.Red);

        var baseError = error.GetBaseException();
        this.Write($"      {baseError.Message}", ConsoleColor.Red);
        this._out.WriteLine();
        this.Write($"      {baseError.GetType().Name} · {seconds:0.0}s", ConsoleColor.DarkGray);
        this._out.WriteLine();
    }

    /// <summary>
    /// Prints the closing summary of a run.
    /// </summary>
    /// <param name="passed">The number of scenarios that passed.</param>
    /// <param name="failed">The number of scenarios that failed.</param>
    /// <param name="shots">The total number of shots captured.</param>
    /// <param name="seconds">The wall-clock duration of the run.</param>
    /// <param name="backend">The graphics backend the run used.</param>
    /// <param name="windowSize">The window size the shots were captured at.</param>
    /// <param name="processes">The number of processes the run spanned.</param>
    public void RunFinished(int passed, int failed, int shots, double seconds, string backend, Vector2Int windowSize, int processes)
    {
        this._out.WriteLine();

        this.Label("Scenarios");
        this.Write($"{passed} passed", ConsoleColor.Green);
        if (failed > 0)
        {
            this.Write(" · ", ConsoleColor.DarkGray);
            this.Write($"{failed} failed", ConsoleColor.Red);
        }

        this._out.WriteLine();

        this.Label("Shots");
        this._out.Write(shots);
        this.Write($"  →  {ScenarioPaths.Shots}", ConsoleColor.DarkGray);
        this._out.WriteLine();

        this.Label("Backend");
        this.Write($"{backend} · {windowSize.X}×{windowSize.Y}", ConsoleColor.DarkGray);
        this._out.WriteLine();

        if (processes > 1)
        {
            this.Label("Processes");
            this._out.Write(processes);
            this._out.WriteLine();
        }

        this.Label("Duration");
        this._out.WriteLine($"{seconds:0.0}s");
    }

    /// <summary>
    /// Reports that this run was saved as the baseline.
    /// </summary>
    /// <param name="baselineDir">The directory the baseline was written to.</param>
    public void BaselineSaved(string baselineDir)
    {
        this._out.WriteLine();
        this.Write($"  Baseline saved to {baselineDir}", ConsoleColor.DarkGray);
        this._out.WriteLine();
    }

    /// <summary>
    /// Reports how this run's shots differ from the baseline.
    /// </summary>
    /// <param name="changes">The shots that changed, are new, or were removed.</param>
    /// <param name="diffDir">The directory the highlight images were written to.</param>
    public void Changes(IReadOnlyList<ShotChange> changes, string diffDir)
    {
        this._out.WriteLine();

        if (changes.Count == 0)
        {
            this.Write("  No visual changes from the baseline.", ConsoleColor.Green);
            this._out.WriteLine();
            return;
        }

        this.Write("  Changes from baseline", ConsoleColor.DarkGray);
        this._out.WriteLine();

        int changed = 0;
        int added = 0;
        int removed = 0;
        foreach (var change in changes)
        {
            switch (change.Kind)
            {
                case ShotChangeKind.Changed:
                    changed++;
                    this.ResultLine('~', ConsoleColor.Yellow, change.RelativePath, $"{change.ChangedFraction * 100:0.0}%", ConsoleColor.Yellow);
                    break;
                case ShotChangeKind.New:
                    added++;
                    this.ResultLine('+', ConsoleColor.Green, change.RelativePath, "new", ConsoleColor.Green);
                    break;
                case ShotChangeKind.Removed:
                    removed++;
                    this.ResultLine('-', ConsoleColor.Red, change.RelativePath, "removed", ConsoleColor.Red);
                    break;
            }
        }

        this._out.WriteLine();
        var parts = new List<string>();
        if (changed > 0)
        {
            parts.Add($"{changed} changed");
        }

        if (added > 0)
        {
            parts.Add($"{added} new");
        }

        if (removed > 0)
        {
            parts.Add($"{removed} removed");
        }

        this.Write($"  {string.Join(" · ", parts)}", ConsoleColor.Yellow);
        this.Write($"  →  {diffDir}/", ConsoleColor.DarkGray);
        this._out.WriteLine();
    }

    /// <summary>
    /// Prints a diagnostic error, such as an unknown scenario or a process that crashed.
    /// </summary>
    /// <param name="message">The message to show.</param>
    public void Error(string message)
    {
        this.Write($"  {message}", ConsoleColor.Red);
        this._out.WriteLine();
    }

    private void ResultLine(char glyph, ConsoleColor glyphColor, string name, string status, ConsoleColor statusColor)
    {
        string left = $"  {glyph} {name}";
        int pad = Math.Max(2, LineWidth - left.Length - status.Length);

        this._out.Write("  ");
        this.Write(glyph.ToString(), glyphColor);
        this._out.Write($" {name}");
        this._out.Write(new string(' ', pad));
        this.Write(status, statusColor);
        this._out.WriteLine();
    }

    private void Label(string text)
    {
        this.Write($"  {text.PadRight(10)}  ", ConsoleColor.DarkGray);
    }

    private void Write(string text, ConsoleColor color)
    {
        if (!this._color)
        {
            this._out.Write(text);
            return;
        }

        var previous = Console.ForegroundColor;
        Console.ForegroundColor = color;
        this._out.Write(text);
        Console.ForegroundColor = previous;
    }

    private static string Plural(int count)
    {
        return count == 1 ? string.Empty : "s";
    }
}
