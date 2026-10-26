using System.Collections.Generic;
using System.Linq;
using Imago.Gui.Scenarios;
using Imago.Scenarios;
using Microsoft.Extensions.DependencyInjection;

namespace Imago.Gui;

/// <summary>
/// Adds the tooltip and cursor layers to the stage of the application, and the GUI tools to scenarios.
/// </summary>
public sealed class GuiModule : Module
{
    /// <inheritdoc />
    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<TooltipLayer>();
        services.AddSingleton<CursorLayer>();
    }

    /// <inheritdoc />
    public override void Boot(Application app)
    {
        app.Stage.AddLayer(app.Services.GetRequiredService<TooltipLayer>());
        app.Stage.AddLayer(app.Services.GetRequiredService<CursorLayer>());
    }

    /// <inheritdoc />
    public override void BootScenario(Scenario scenario)
    {
        scenario.AddTool(new GuiScenarioTools(scenario));
    }
}

/// <summary>
/// Provides the GUI extensions for the core types.
/// </summary>
public static class GuiExtensions
{
    extension(Stage stage)
    {
        /// <summary>
        /// Gets the tooltip layer on this stage, or null if there is none.
        /// </summary>
        public TooltipLayer? TooltipLayer => stage.FindLayer<TooltipLayer>();

        /// <summary>
        /// Gets the cursor layer on this stage, or null if there is none.
        /// </summary>
        public CursorLayer? CursorLayer => stage.FindLayer<CursorLayer>();
    }

    extension(Screen screen)
    {
        /// <summary>
        /// Gets the primary GUI layer of the screen (the first one added), if any.
        /// </summary>
        public GuiLayer? GuiLayer => screen.FindLayer<GuiLayer>();

        /// <summary>
        /// Gets all GUI layers in the screen, ordered by ZOrder.
        /// </summary>
        public IEnumerable<GuiLayer> GuiLayers => screen.Layers.OfType<GuiLayer>().OrderBy(l => l.ZOrder);
    }

    extension(Scenario scenario)
    {
        /// <summary>
        /// Gets the GUI tools of this scenario.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the application has no <see cref="GuiModule"/>.</exception>
        public GuiScenarioTools Gui => scenario.Tool<GuiScenarioTools>();
    }
}
