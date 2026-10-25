using Imago.Scenarios;

namespace Imago.Gui.Scenarios;

/// <summary>
/// Provides GUI extensions for <see cref="Scenario"/>.
/// </summary>
public static class ScenarioGuiExtensions
{
    extension(Scenario scenario)
    {
        /// <summary>
        /// Gets the GUI tools of this scenario.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the application has no <see cref="GuiModule"/>.</exception>
        public GuiScenarioTools Gui => scenario.Tool<GuiScenarioTools>();
    }
}
