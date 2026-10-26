using Imago.Input;
using Imago.Metrics;
using Imago.Rendering;
using Microsoft.Extensions.DependencyInjection;
using NeoVeldrid;
using Texture = Imago.Textures.Texture;

namespace Imago.DearImGui;

/// <summary>
/// Adds the Dear ImGui layer to the stage of the application.
/// </summary>
public sealed class ImGuiModule : Module
{
    private bool _hasMetricsOverlay;

    private Key? _metricsOverlayToggleKey;

    /// <summary>
    /// Adds the <see cref="MetricsOverlay"/>, a window with the metrics of the application. It starts hidden.
    /// </summary>
    /// <param name="toggleKey">The key that shows and hides the window, or null to show and hide it only from code.</param>
    /// <returns>This module.</returns>
    public ImGuiModule WithMetricsOverlay(Key? toggleKey = null)
    {
        this._hasMetricsOverlay = true;
        this._metricsOverlayToggleKey = toggleKey;
        return this;
    }

    /// <inheritdoc />
    public override void Register(IServiceCollection services)
    {
        services.AddSingleton<ImGuiLayer>();

        if (this._hasMetricsOverlay)
        {
            services.AddSingleton(sp => new MetricsOverlay(
                sp.GetRequiredService<ImGuiLayer>(),
                sp.GetRequiredService<FrameMetrics>(),
                sp.GetRequiredService<Renderer>(),
                sp.GetRequiredService<InputManager>(),
                this._metricsOverlayToggleKey
            ));
        }
    }

    /// <inheritdoc />
    public override void Boot(Application app)
    {
        var metrics = app.Metrics.Add(new ImGuiMetrics());
        app.Metrics.Layout.Time("Update/Layers/ImGui", metrics.UpdateTime);

        app.Stage.AddLayer(app.Services.GetRequiredService<ImGuiLayer>());

        if (this._hasMetricsOverlay)
        {
            app.Services.GetRequiredService<MetricsOverlay>();
        }
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

    extension(FrameMetrics metrics)
    {
        /// <summary>
        /// Gets the metrics of the ImGui layer.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the application has no <see cref="ImGuiModule"/>.</exception>
        public ImGuiMetrics ImGui => metrics.Group<ImGuiMetrics>();
    }
}
