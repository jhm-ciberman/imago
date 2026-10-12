using Microsoft.Extensions.DependencyInjection;

namespace Imago.DearImGui;

/// <summary>
/// Adds the Dear ImGui layer to the stage of the application.
/// </summary>
public sealed class ImGuiModule : Module
{
    /// <inheritdoc />
    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<ImGuiLayer>();
    }

    /// <inheritdoc />
    public override void Boot(Application app)
    {
        app.Stage.AddLayer(app.Services.GetRequiredService<ImGuiLayer>());
    }
}
