using Imago.Graphics3D.Metrics;
using Imago.Graphics3D.Rendering;
using Imago.Graphics3D.Scenarios;
using Imago.Graphics3D.SceneGraph;
using Imago.Metrics;
using Imago.Scenarios;
using Microsoft.Extensions.DependencyInjection;

namespace Imago.Graphics3D;

/// <summary>
/// Adds the 3D renderer, so the application can render 3D scenes, its metrics, and the 3D scene tools to scenarios.
/// </summary>
public sealed class Graphics3DModule : Module
{
    /// <inheritdoc />
    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<Forward3DRenderer>();
    }

    /// <inheritdoc />
    public override void Boot(Application app)
    {
        app.Metrics.Add(new Graphics3DMetrics());
        app.Services.GetRequiredService<Forward3DRenderer>();
    }

    /// <inheritdoc />
    public override void BootScenario(Scenario scenario)
    {
        scenario.AddTool(new Scene3DScenarioTools(scenario));
    }
}

/// <summary>
/// Provides the 3D extensions for the core types.
/// </summary>
public static class Graphics3DExtensions
{
    extension(Stage stage)
    {
        /// <summary>
        /// Gets the first 3D scene on the stage, or null if the stage has none.
        /// </summary>
        public Scene3DLayer? Scene3D => stage.FindLayer<Scene3DLayer>();
    }

    extension(Screen screen)
    {
        /// <summary>
        /// Gets the first 3D scene of the screen, or null if the screen has none.
        /// </summary>
        public Scene3DLayer? Scene3D => screen.FindLayer<Scene3DLayer>();
    }

    extension(Scenario scenario)
    {
        /// <summary>
        /// Gets the 3D scene tools of this scenario.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the application has no <see cref="Graphics3DModule"/>.</exception>
        public Scene3DScenarioTools Scene3D => scenario.Tool<Scene3DScenarioTools>();
    }

    extension(FrameMetrics metrics)
    {
        /// <summary>
        /// Gets the metrics of the 3D renderer.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the application has no <see cref="Graphics3DModule"/>.</exception>
        public Graphics3DMetrics Graphics3D => metrics.Group<Graphics3DMetrics>();
    }
}
