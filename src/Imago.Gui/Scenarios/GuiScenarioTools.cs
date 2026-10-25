using System.Collections.Generic;
using System.Linq;
using Imago.Scenarios;

namespace Imago.Gui.Scenarios;

/// <summary>
/// The GUI tools of a running <see cref="Imago.Scenarios.Scenario"/>. Finds search every visible GUI layer.
/// </summary>
public sealed class GuiScenarioTools : GuiFinder
{
    internal GuiScenarioTools(Scenario scenario)
        : base(scenario)
    {
    }

    private protected override IEnumerable<Visual> GetRoots()
    {
        return this.Scenario.App.Stage.FindVisibleLayers<GuiLayer>()
            .Select(layer => layer.Content)
            .OfType<Control>();
    }

    private protected override string Describe(string step)
    {
        return step;
    }
}
