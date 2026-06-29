using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Imago.ScenarioRunner;

/// <summary>
/// How a shot differs from its baseline.
/// </summary>
internal enum ShotChangeKind
{
    /// <summary>The shot exists in both and its pixels differ.</summary>
    Changed,

    /// <summary>The shot was captured in this run but not in the baseline.</summary>
    New,

    /// <summary>The shot is in the baseline but was not captured in this run.</summary>
    Removed,
}

/// <summary>
/// A single shot that differs from the baseline.
/// </summary>
/// <param name="RelativePath">The shot's path relative to the captures root, like <c>town-map/00_town-map.png</c>.</param>
/// <param name="Kind">How it differs.</param>
/// <param name="ChangedFraction">The fraction of pixels that changed, for <see cref="ShotChangeKind.Changed"/>.</param>
internal readonly record struct ShotChange(string RelativePath, ShotChangeKind Kind, double ChangedFraction);

/// <summary>
/// Compares a run's shots against a baseline and writes a highlight image for each that changed.
/// </summary>
internal static class ScenarioComparer
{
    // A generous per-channel tolerance absorbs encoding and anti-aliasing jitter; a small area threshold keeps
    // a stray pixel or two from being reported as a change. Tune these if real changes slip through or noise creeps in.
    private const int ChannelTolerance = 8;
    private const double ChangedThreshold = 0.0005;

    /// <summary>
    /// Compares every shot under <paramref name="currentDir"/> against the matching one under
    /// <paramref name="baselineDir"/>, writing a highlight image for each change into <paramref name="diffDir"/>.
    /// </summary>
    /// <param name="baselineDir">The baseline captures root.</param>
    /// <param name="currentDir">This run's captures root.</param>
    /// <param name="diffDir">The directory to write highlight images into; cleared first.</param>
    /// <returns>The shots that changed, are new, or were removed. Unchanged shots are not included.</returns>
    public static IReadOnlyList<ShotChange> Compare(string baselineDir, string currentDir, string diffDir)
    {
        if (Directory.Exists(diffDir))
        {
            Directory.Delete(diffDir, recursive: true);
        }

        var current = ShotsIn(currentDir);
        var baseline = ShotsIn(baselineDir);
        var changes = new List<ShotChange>();

        foreach (string relativePath in current)
        {
            if (!baseline.Contains(relativePath))
            {
                changes.Add(new ShotChange(relativePath, ShotChangeKind.New, 1.0));
                continue;
            }

            double fraction = CompareShot(
                Path.Combine(baselineDir, relativePath),
                Path.Combine(currentDir, relativePath),
                Path.Combine(diffDir, relativePath));

            if (fraction > ChangedThreshold)
            {
                changes.Add(new ShotChange(relativePath, ShotChangeKind.Changed, fraction));
            }
        }

        foreach (string relativePath in baseline)
        {
            if (!current.Contains(relativePath))
            {
                changes.Add(new ShotChange(relativePath, ShotChangeKind.Removed, 1.0));
            }
        }

        return changes;
    }

    private static HashSet<string> ShotsIn(string root)
    {
        var shots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!Directory.Exists(root))
        {
            return shots;
        }

        foreach (string file in Directory.EnumerateFiles(root, "*.png", SearchOption.AllDirectories))
        {
            shots.Add(Path.GetRelativePath(root, file));
        }

        return shots;
    }

    private static double CompareShot(string baselinePath, string currentPath, string diffPath)
    {
        using var baseline = Image.Load<Rgba32>(baselinePath);
        using var current = Image.Load<Rgba32>(currentPath);

        if (baseline.Width != current.Width || baseline.Height != current.Height)
        {
            // A resized shot can't be compared pixel for pixel; treat it as fully changed and save the new one.
            Save(current, diffPath);
            return 1.0;
        }

        int width = current.Width;
        int height = current.Height;
        using var highlight = new Image<Rgba32>(width, height);
        long changed = 0;

        baseline.ProcessPixelRows(current, highlight, (baselineRows, currentRows, highlightRows) =>
        {
            for (int y = 0; y < height; y++)
            {
                Span<Rgba32> baselineRow = baselineRows.GetRowSpan(y);
                Span<Rgba32> currentRow = currentRows.GetRowSpan(y);
                Span<Rgba32> highlightRow = highlightRows.GetRowSpan(y);

                for (int x = 0; x < width; x++)
                {
                    if (Differs(baselineRow[x], currentRow[x]))
                    {
                        changed++;
                        highlightRow[x] = new Rgba32(255, 0, 255, 255);
                    }
                    else
                    {
                        Rgba32 pixel = currentRow[x];
                        highlightRow[x] = new Rgba32((byte)(pixel.R / 4), (byte)(pixel.G / 4), (byte)(pixel.B / 4), 255);
                    }
                }
            }
        });

        double fraction = (double)changed / (width * height);
        if (fraction > ChangedThreshold)
        {
            Save(highlight, diffPath);
        }

        return fraction;
    }

    private static bool Differs(Rgba32 a, Rgba32 b)
    {
        return Math.Abs(a.R - b.R) > ChannelTolerance
            || Math.Abs(a.G - b.G) > ChannelTolerance
            || Math.Abs(a.B - b.B) > ChannelTolerance;
    }

    private static void Save(Image image, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        image.SaveAsPng(path);
    }
}
