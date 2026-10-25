using Imago.Scenarios;

namespace Imago.Graphics3D.Scenarios;

/// <summary>
/// Provides 3D scene extensions for <see cref="Scenario"/>.
/// </summary>
public static class ScenarioScene3DExtensions
{
    extension(Scenario scenario)
    {
        /// <summary>
        /// Gets the 3D scene tools of this scenario.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the application has no <see cref="Graphics3DModule"/>.</exception>
        public Scene3DScenarioTools Scene3D => scenario.Tool<Scene3DScenarioTools>();
    }
}
