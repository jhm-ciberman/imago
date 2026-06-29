using System.Globalization;
using System.IO;
using Imago.Support.Numerics;

namespace Imago.ScenarioRunner;

/// <summary>
/// A worker's batch results, written to a file so the orchestrating parent can fold them into one summary.
/// </summary>
/// <remarks>
/// Workers stream their own progress to the shared console; this carries only the totals the parent can't see
/// from the outside, so it can render a unified summary across every process.
/// </remarks>
internal readonly record struct ScenarioReport(int Passed, int Failed, int Shots, string Backend, Vector2Int WindowSize)
{
    /// <summary>
    /// Writes the report to the given path as a single tab-separated line.
    /// </summary>
    /// <param name="path">The file to write.</param>
    public void Write(string path)
    {
        string line = string.Join('\t', this.Passed, this.Failed, this.Shots, this.Backend, this.WindowSize.X, this.WindowSize.Y);
        File.WriteAllText(path, line);
    }

    /// <summary>
    /// Reads a report previously written by a worker, or null if the file is missing or malformed (a worker
    /// that crashed before writing it).
    /// </summary>
    /// <param name="path">The file to read.</param>
    /// <returns>The parsed report, or null.</returns>
    public static ScenarioReport? Read(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var fields = File.ReadAllText(path).Split('\t');
        if (fields.Length != 6
            || !int.TryParse(fields[0], out int passed)
            || !int.TryParse(fields[1], out int failed)
            || !int.TryParse(fields[2], out int shots)
            || !int.TryParse(fields[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out int width)
            || !int.TryParse(fields[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out int height))
        {
            return null;
        }

        return new ScenarioReport(passed, failed, shots, fields[3], new Vector2Int(width, height));
    }
}
