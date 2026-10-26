using Imago.Textures;
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

/// <summary>
/// Provides the ImGui extensions for the core types.
/// </summary>
public static class ImGuiExtensions
{
    extension(Texture texture)
    {
        /// <summary>
        /// Gets the ImGui binding for this texture, allowing it to be displayed in an ImGui UI.
        /// </summary>
        public nint ImGuiBinding => ImGuiLayer.Instance.GetOrCreateTextureBinding(texture);
    }
}
