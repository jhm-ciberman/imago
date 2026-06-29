using System.Collections.Generic;
using NeoVeldrid;

namespace Imago.ScenarioRunner;

/// <summary>
/// Groups scenarios into batches that can share one process.
/// </summary>
internal static class ScenarioBatcher
{
    /// <summary>
    /// Groups the scenarios into batches. Scenarios that share a backend batch together; an isolated scenario,
    /// or any scenario whose backend differs, gets its own batch.
    /// </summary>
    /// <param name="scenarios">The scenarios to group.</param>
    /// <param name="cliBackend">The backend override from the command line used to resolve grouping, or null.</param>
    /// <param name="forceIsolated">Whether every scenario should be its own batch.</param>
    /// <returns>The batches, in discovery order.</returns>
    public static List<List<ScenarioDescriptor>> Group(
        IReadOnlyList<ScenarioDescriptor> scenarios,
        GraphicsBackend? cliBackend,
        bool forceIsolated)
    {
        var batches = new List<List<ScenarioDescriptor>>();
        var byBackend = new Dictionary<string, List<ScenarioDescriptor>>();

        foreach (var scenario in scenarios)
        {
            if (forceIsolated || scenario.Isolated)
            {
                batches.Add([scenario]);
                continue;
            }

            // The renderer is built once per process, so a differing backend can't share one; a null backend
            // means "let the host pick", which resolves the same for all of them, so they group.
            GraphicsBackend? backend = scenario.Backend ?? cliBackend;
            string key = backend?.ToString() ?? string.Empty;
            if (!byBackend.TryGetValue(key, out var batch))
            {
                batch = [];
                byBackend[key] = batch;
                batches.Add(batch);
            }

            batch.Add(scenario);
        }

        return batches;
    }
}
