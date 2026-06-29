using System.IO;

namespace Imago.ScenarioRunner;

/// <summary>
/// The on-disk layout of a scenario run's output, all under one folder.
/// </summary>
internal static class ScenarioPaths
{
    /// <summary>The root for everything a scenario run produces.</summary>
    public static readonly string Root = Path.Combine("artifacts", "scenarios");

    /// <summary>This run's captures, as <c>&lt;scenario&gt;/NN_label.png</c>.</summary>
    public static readonly string Shots = Path.Combine(Root, "shots");

    /// <summary>The blessed baseline, mirroring <see cref="Shots"/>.</summary>
    public static readonly string Baseline = Path.Combine(Root, "baseline");

    /// <summary>Highlight images for shots that changed, mirroring <see cref="Shots"/>.</summary>
    public static readonly string Diff = Path.Combine(Root, "diff");

    /// <summary>The game's redirected console output for the run.</summary>
    public static readonly string Log = Path.Combine(Root, "run.log");
}
