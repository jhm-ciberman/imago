using Microsoft.Extensions.DependencyInjection;

namespace Imago.Gui;

/// <summary>
/// Adds the tooltip and cursor layers to the stage of the application.
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
}
